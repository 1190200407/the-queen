using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
namespace ComicChess.TheQueen;

public class FriendlyAmalgam : QueenMinionModel
{
    /// <summary>与默认沉睡、紧急避险、承伤击晕等共用同一展示状态。</summary>
    internal static readonly MoveState SleepOverlayMoveState = new(
        "AMALGAM_SLEEP",
        _ => Task.CompletedTask,
        new AmalgamSleepIntent());

    private const int TorchSlotCount = 3;

    /// <summary>终焉形态：仅用于意图条 UI 的壳状态；<see cref="MoveState.PerformMove"/> 不会在友方宠路径被调用，委托保持空操作即可。</summary>
    private MoveState? _terminusDisplayShell;

    private readonly List<AbstractIntent> _terminusDisplayScratch = new();

    private static readonly MethodInfo TerminusMoveStateIntentsSetter =
        typeof(MoveState).GetProperty(nameof(MoveState.Intents))?.GetSetMethod(nonPublic: true)
        ?? throw new InvalidOperationException("MoveState.Intents has no non-public setter.");

    /// <summary>每个灯槽一条已学意图；按槽 0→1→2 顺序写入空槽，意图不随执行而清除。学习后当前灯立即切到该槽；三槽满时学习不写入，当场执行该意图一次。</summary>
    private readonly AmalgamActionModel?[] _intentByTorchSlot = new AmalgamActionModel?[TorchSlotCount];

    /// <summary>当前轮到执行的灯槽下标（仅在已点亮的槽之间轮转）。</summary>
    private int _currentTorchSlotIndex;

    /// <summary>当前灯槽对应的意图（用于展示与 <see cref="BeforeTurnEnd"/> 执行）。</summary>
    public AmalgamActionModel? LearnedAction => _intentByTorchSlot[_currentTorchSlotIndex];

    /// <summary>当前即将执行的灯槽。无已学意图时返回 -1。</summary>
    public int CurrentTorchSlotIndex => LearnedAction == null ? -1 : _currentTorchSlotIndex;

    private AmalgamForcedActionModel? _forcedAction;

    public bool HasIntentInTorchSlot(int slotIndex) =>
        slotIndex >= 0 && slotIndex < TorchSlotCount && _intentByTorchSlot[slotIndex] != null;

    /// <summary>
    /// 小火 UI：存活、无强制行动、且非 <see cref="IsBodyguardSleeping"/> 时，视为在用灯槽记录的意图（当前槽紫）；否则已学槽统一绿。
    /// </summary>
    public bool IsUsingTorchRecordedIntentForVisuals =>
        Creature.IsAlive && _forcedAction == null && !IsBodyguardSleeping();

    /// <summary>
    /// 是否视为「沉睡」而不替主人承伤：已死亡、紧急避险等 <see cref="AmalgamForcedActionModel.IsSleepingForBodyguard"/>，
    /// 或灯槽即将执行的意图为沉睡。
    /// </summary>
    public bool IsBodyguardSleeping()
    {
        Creature self = Creature;
        if (!self.IsAlive)
        {
            return true;
        }

        if (_forcedAction?.IsSleepingForBodyguard == true)
        {
            return true;
        }

        if (LearnedAction != null)
        {
            return IsSleepPendingMoveState(LearnedAction.GetMoveStateForDisplay(self));
        }

        return true;
    }

    /// <summary>进入强制行动；下回合由 <see cref="EmergencyEvasionPendingPower"/> 等逻辑调用 <see cref="ClearForcedAction"/>。</summary>
    public async Task BeginForcedAction(AmalgamForcedActionModel? action)
    {
        if (action == null)
        {
            ClearForcedAction();
            return;
        }

        _forcedAction = action;
        RefreshDisplayedIntent();
        FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(Creature);
        await _forcedAction.OnBeginAsync(Creature);
    }

    public void ClearForcedAction()
    {
        _forcedAction = null;
        RefreshDisplayedIntent();
        FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(Creature);
    }

    private MoveState GetPendingDisplayedMoveState(Creature self)
    {
        if (_forcedAction != null)
        {
            return _forcedAction.GetOverlayMoveState(self);
        }

        if (LearnedAction != null &&
            self.PetOwner is Player { Creature: var queenBody } &&
            queenBody.GetPower<TerminusFormPower>() is { Amount: > 0 } terminus)
        {
            MoveState? combined = TryGetTerminusCombinedDisplayMoveState(self, (int)terminus.Amount);
            if (combined != null)
            {
                return combined;
            }
        }

        if (LearnedAction != null)
        {
            return LearnedAction.GetMoveStateForDisplay(self);
        }

        return SleepOverlayMoveState;
    }

    /// <summary>
    /// 按 <see cref="BeforeTurnEnd"/> 相同轮数与灯槽轮转，把每一动要展示的 <see cref="AbstractIntent"/> 拼进 <paramref name="buffer"/>。
    /// </summary>
    private void BuildTerminusDisplayIntentScratch(Creature self, List<AbstractIntent> buffer, int totalPasses)
    {
        buffer.Clear();
        int idx = _currentTorchSlotIndex;
        for (int pass = 0; pass < totalPasses; pass++)
        {
            AmalgamActionModel? action = _intentByTorchSlot[idx];
            if (action == null)
            {
                break;
            }

            MoveState moveState = action.GetMoveStateForDisplay(self);
            if (IsSleepPendingMoveState(moveState))
            {
                break;
            }

            foreach (AbstractIntent intent in moveState.Intents)
            {
                buffer.Add(intent);
            }

            idx = GetNextLitTorchIndexAfter(idx);
        }
    }

    /// <summary>与 <see cref="RotateCurrentToNextLitTorchSlot"/> 相同的「下一盏已学槽」下标，但不改当前灯指针字段。</summary>
    private int GetNextLitTorchIndexAfter(int fromIndex)
    {
        if (_intentByTorchSlot[fromIndex] == null)
        {
            return fromIndex;
        }

        for (int step = 1; step <= TorchSlotCount; step++)
        {
            int idx = (fromIndex + step) % TorchSlotCount;
            if (_intentByTorchSlot[idx] != null)
            {
                return idx;
            }
        }

        return fromIndex;
    }

    private MoveState GetOrCreateTerminusDisplayShell()
    {
        if (_terminusDisplayShell != null)
        {
            return _terminusDisplayShell;
        }

        _terminusDisplayShell = new MoveState(
            "AMALGAM_MULTIPLE_END_TURN",
            static _ => Task.CompletedTask,
            Array.Empty<AbstractIntent>());
        return _terminusDisplayShell;
    }

    private static void AssignTerminusMoveStateIntents(MoveState state, IReadOnlyList<AbstractIntent> intents) =>
        TerminusMoveStateIntentsSetter.Invoke(state, new object[] { intents });

    /// <summary>终焉形态下合并多动意图条；失败时返回 <c>null</c> 让调用方回退到单槽展示。</summary>
    private MoveState? TryGetTerminusCombinedDisplayMoveState(Creature self, int extraEndTurnActs)
    {
        int totalPasses = 1 + extraEndTurnActs;
        BuildTerminusDisplayIntentScratch(self, _terminusDisplayScratch, totalPasses);
        if (_terminusDisplayScratch.Count == 0)
        {
            return null;
        }

        MoveState shell = GetOrCreateTerminusDisplayShell();
        AssignTerminusMoveStateIntents(shell, _terminusDisplayScratch.ToArray());
        return shell;
    }

    private static bool IsSleepPendingMoveState(MoveState state)
    {
        if (state.Intents.Count == 0)
        {
            return true;
        }

        return state.Intents[0] is AmalgamSleepIntent;
    }

    public AmalgamActionModel? GetIntentInTorchSlot(int slotIndex) =>
        slotIndex >= 0 && slotIndex < TorchSlotCount ? _intentByTorchSlot[slotIndex] : null;

    /// <summary>灯槽悬停：与意图节点悬停同源，按槽取 <see cref="AmalgamActionModel.GetMoveStateForDisplay"/> 再 <see cref="AbstractIntent.GetHoverTip"/>。</summary>
    public bool TryGetTorchSlotHoverTip(Creature amalgamCreature, int slotIndex, out HoverTip hoverTip)
    {
        hoverTip = default;
        if (slotIndex < 0 || slotIndex >= TorchSlotCount ||
            _intentByTorchSlot[slotIndex] is not { } action ||
            amalgamCreature.CombatState is not { } combatState)
        {
            return false;
        }

        MoveState moveState = action.GetMoveStateForDisplay(amalgamCreature);
        if (moveState.Intents.Count == 0)
        {
            return false;
        }

        IEnumerable<Creature> targets = combatState.Players.Select(static p => p.Creature);
        if (action is AmalgamEmptyCupIntentAction && moveState.Intents.Count > 1)
        {
            LocString title = new("monsters", "FRIENDLY_AMALGAM.intent_empty_cup.title");
            LocString desc = new("monsters", "FRIENDLY_AMALGAM.intent_empty_cup.description");
            desc.Add("IsMultiplayer", combatState.RunState.Players.Count > 1);
            AmalgamIntentEnergyLoc.AddEnergyPrefixFromPetOwner(desc, amalgamCreature);
            AbstractIntent iconSource = moveState.Intents[0];
            hoverTip = new HoverTip(title, desc, iconSource.GetTexture(targets, amalgamCreature));
            return true;
        }

        AbstractIntent firstIntent = moveState.Intents[0];
        hoverTip = firstIntent.GetHoverTip(targets, amalgamCreature);
        return true;
    }

    public override int MaxInitialHp => 0;
    public override int MinInitialHp => 0;

    /// <summary>与 <see cref="MegaCrit.Sts2.Core.Models.Monsters.Osty"/> 一致：0 血/尸体时不显示血条；复活后由 <see cref="FriendlyAmalgamCmd.SyncHealthBarVisibility"/> 再打开。</summary>
    public override bool IsHealthBarVisible => Creature.IsAlive;

    protected override string VisualsPath => "res://TheQueen/scenes/creature_visuals/torch_head_amalgam_minion.tscn";

    public static readonly string IdleAnimName = "idle_loop";
    public static readonly string DeathAnimName = "die";
    public static readonly string BuffAnimName = "buff";
    public static readonly string CastAnimName = "buff";
    public static readonly string HitAnimName = "hurt";
    public static readonly string DebuffAnimName = "_ignore/hug2";
    public static readonly string AttackAnimName = "attack";
    public static readonly string PowerAttackAnimName = "debuff";
    public static readonly string SleepAnimName = "_ignore/string_rigging";

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new(IdleAnimName, isLooping: true);
        AnimState attack = new(AttackAnimName);
        AnimState buff = new(BuffAnimName);
        AnimState cast = new(CastAnimName);
        AnimState hurt = new(HitAnimName);
        AnimState debuff = new(DebuffAnimName);
        AnimState powerAttack = new(PowerAttackAnimName);
        AnimState sleep = new(SleepAnimName, isLooping: true);
        AnimState death = new(DeathAnimName);

        attack.NextState = idle;
        buff.NextState = idle;
        cast.NextState = idle;
        hurt.NextState = idle;
        debuff.NextState = idle;
        powerAttack.NextState = idle;

        CreatureAnimator animator = new(idle, controller);

        // 兼容引擎与常见调用习惯。
        animator.AddAnyState("Idle", idle);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Buff", buff);
        animator.AddAnyState("Cast", cast);
        animator.AddAnyState("Hit", hurt);
        animator.AddAnyState("Debuff", debuff);
        animator.AddAnyState("Dead", death);
        animator.AddAnyState("PowerAttack", powerAttack);
        animator.AddAnyState("Sleep", sleep);

        return animator;
    }

    public override async Task OnSummon(Player owner, Creature self, MinionSummonOptions options)
    {
        await ClearTorchSlots();
    }

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        await base.BeforeTurnEnd(choiceContext, side);

        Creature self = Creature;
        if (side != CombatSide.Player)
        {
            return;
        }

        // 击倒沉睡占一回合：先结算沉睡行动（1 血、无治疗演出），本回合末不执行灯槽意图。
        if (!self.IsAlive && self.GetPower<AmalgamDieForYouPower>() is { } deathSleepPower &&
            deathSleepPower.IsAwaitingDeathSleepRevive)
        {
            await deathSleepPower.ExecuteDeathSleepReviveSilentlyAsync(choiceContext, self);
            return;
        }

        if (!self.IsAlive)
        {
            return;
        }

        SyncCurrentTorchSlotIfNeeded();

        if (_forcedAction?.SkipsPlayerTurnEndTorchExecution == true)
        {
            RefreshDisplayedIntent();
            FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(self);
            return;
        }

        int extraEndTurnActs = 0;
        if (self.PetOwner is Player { Creature: var queenBody })
        {
            TerminusFormPower? terminus = queenBody.GetPower<TerminusFormPower>();
            if (terminus != null)
            {
                extraEndTurnActs = (int)terminus.Amount;
            }
        }

        int endTurnPasses = 1 + extraEndTurnActs;

        for (int pass = 0; pass < endTurnPasses; pass++)
        {
            if (!self.IsAlive)
            {
                break;
            }

            SyncCurrentTorchSlotIfNeeded();

            AmalgamActionModel? action = LearnedAction;
            if (action == null)
            {
                break;
            }

            if (IsSleepPendingMoveState(action.GetMoveStateForDisplay(self)))
            {
                if (pass == 0)
                {
                    RefreshDisplayedIntent();
                    FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(self);
                }

                break;
            }

            await FriendlyAmalgamCmd.TryPerformIntent(self);
            await action.ExecuteAsync(choiceContext, self);
            RotateCurrentToNextLitTorchSlot();
            FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(self);
            // 最后一动再刷新意图，因为前几动会同时显示所有的意图
            if (pass == endTurnPasses - 1)
            {
                RefreshDisplayedIntent();
            }
        }
    }

    /// <summary>立刻执行当前灯槽记录的意图（意图条演出 + 结算），<strong>不</strong>清空槽位、不轮转。</summary>
    public async Task ActCurrentIntentImmediatelyAsync(PlayerChoiceContext choiceContext)
    {
        Creature self = Creature;
        if (!self.IsAlive)
        {
            return;
        }

        SyncCurrentTorchSlotIfNeeded();

        if (_forcedAction?.SkipsPlayerTurnEndTorchExecution == true)
        {
            return;
        }

        AmalgamActionModel? action = LearnedAction;
        if (action == null)
        {
            return;
        }

        if (IsSleepPendingMoveState(action.GetMoveStateForDisplay(self)))
        {
            return;
        }

        await FriendlyAmalgamCmd.TryPerformIntent(self);
        await action.ExecuteAsync(choiceContext, self);
    }

    /// <summary>清空当前灯槽内意图，将「当前灯」切到下一盏有记录的槽（无则沉睡展示）；用于断念等仅遗忘、或已在外部执行过意图后的遗忘。</summary>
    public async Task ForgetCurrentTorchSlotIntentAsync()
    {
        Creature self = Creature;
        if (!self.IsAlive)
        {
            return;
        }

        SyncCurrentTorchSlotIfNeeded();

        if (_forcedAction?.SkipsPlayerTurnEndTorchExecution == true)
        {
            return;
        }

        _intentByTorchSlot[_currentTorchSlotIndex] = null;

        bool foundNext = false;
        for (int step = 1; step <= TorchSlotCount; step++)
        {
            int idx = (_currentTorchSlotIndex + step) % TorchSlotCount;
            if (_intentByTorchSlot[idx] != null)
            {
                _currentTorchSlotIndex = idx;
                foundNext = true;
                break;
            }
        }

        if (!foundNext)
        {
            _currentTorchSlotIndex = 0;
            await CreatureCmd.TriggerAnim(self, "Sleep", 0f);
        }
        else
        {
            await FriendlyAmalgamCmd.AwakeAsync(self);
        }

        RefreshDisplayedIntent();
        FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(self);
    }

    public async Task LearnIntent(PlayerChoiceContext choiceContext, AmalgamActionModel intent)
    {
        int emptySlot = FirstEmptyTorchSlotIndex();
        if (emptySlot < 0)
        {
            // 三槽已满：不写入槽位，当场执行本次要学的意图；不做意图条/小火等意图 UI 同步。
            await intent.ExecuteAsync(choiceContext, Creature);
            return;
        }

        bool hadAnyIntentBefore = false;
        for (int i = 0; i < TorchSlotCount; i++)
        {
            if (_intentByTorchSlot[i] != null)
            {
                hadAnyIntentBefore = true;
                break;
            }
        }

        _intentByTorchSlot[emptySlot] = intent;
        _currentTorchSlotIndex = emptySlot;

        RefreshDisplayedIntent();
        FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(Creature);

        // 仅 0 意图 → 第 1 条意图（Sleep）时切到 Idle；再学新意图不刷 Idle。
        if (!hadAnyIntentBefore)
        {
            await CreatureCmd.TriggerAnim(Creature, "Idle", 0f);
        }
    }

    private async Task ClearTorchSlots()
    {
        _forcedAction = null;
        for (int i = 0; i < TorchSlotCount; i++)
        {
            _intentByTorchSlot[i] = null;
        }

        _currentTorchSlotIndex = 0;
        RefreshDisplayedIntent();
        FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(Creature);
        await CreatureCmd.TriggerAnim(Creature, "Sleep", 0f);
    }

    private int FirstEmptyTorchSlotIndex()
    {
        for (int i = 0; i < TorchSlotCount; i++)
        {
            if (_intentByTorchSlot[i] == null)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>若当前槽无意图但别处已点亮，把当前指针拉回第一个亮槽（防御性同步）。</summary>
    private void SyncCurrentTorchSlotIfNeeded()
    {
        if (_intentByTorchSlot[_currentTorchSlotIndex] != null)
        {
            return;
        }

        for (int i = 0; i < TorchSlotCount; i++)
        {
            if (_intentByTorchSlot[i] != null)
            {
                _currentTorchSlotIndex = i;
                return;
            }
        }
    }

    /// <summary>执行完当前槽后，切到下一个已点亮的槽（仅在这些槽之间循环）。</summary>
    private void RotateCurrentToNextLitTorchSlot()
    {
        _currentTorchSlotIndex = GetNextLitTorchIndexAfter(_currentTorchSlotIndex);
    }

    internal void RefreshDisplayedIntent()
    {
        Creature self = Creature;
        MoveState state = GetPendingDisplayedMoveState(self);
        SetMoveImmediate(state, forceTransition: true);
    }
}