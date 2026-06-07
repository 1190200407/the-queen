using System;
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
using MegaCrit.Sts2.Core.Models;
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
    private AmalgamActionModel?[] _intentByTorchSlot = new AmalgamActionModel?[TorchSlotCount];

    /// <summary>当前轮到执行的灯槽下标（仅在已点亮的槽之间轮转）。</summary>
    private int _currentTorchSlotIndex;

    /// <summary>当前灯槽对应的意图（用于展示与 <see cref="BeforeTurnEnd"/> 执行）。</summary>
    public AmalgamActionModel? LearnedAction => _currentTorchSlotIndex >= 0 && _currentTorchSlotIndex < TorchSlotCount ? _intentByTorchSlot[_currentTorchSlotIndex] : null;

    /// <summary>当前即将执行的灯槽。无已学意图时返回 -1。</summary>
    public int CurrentTorchSlotIndex => _currentTorchSlotIndex;

    /// <summary>
    /// 原版 <see cref="AbstractModel.MutableClone"/> 为 <c>MemberwiseClone</c> + <see cref="DeepCloneFields"/>；
    /// 未重写的引用类型字段会与源（含 ModelDb 原型）共用同一引用。
    /// 召唤时为每只宠物对怪物模型做 <c>ToMutable()</c>（原版 <c>MegaCrit.Sts2.Core.Commands.PlayerCmd.AddPet&lt;T&gt;</c>），
    /// 若不拆数组则多只 <see cref="FriendlyAmalgam"/> 会共用同一条 <c>_intentByTorchSlot</c>。
    /// </summary>
    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        AmalgamActionModel?[] copiedFrom = _intentByTorchSlot;
        _intentByTorchSlot = new AmalgamActionModel?[TorchSlotCount];
        if (copiedFrom != null && copiedFrom.Length >= TorchSlotCount)
        {
            Array.Copy(copiedFrom, _intentByTorchSlot, TorchSlotCount);
        }
    }

    private AmalgamForcedActionModel? _forcedAction;

    public bool HasIntentInTorchSlot(int slotIndex) =>
        slotIndex >= 0 && slotIndex < TorchSlotCount && _intentByTorchSlot[slotIndex] != null;

    /// <summary>三盏灯槽均已记录意图（满槽后下一次学习会先当场执行一次再写入）。</summary>
    public bool HasAllTorchSlotsFilled => FirstEmptyTorchSlotIndex() < 0;

    #region Sleep
    [Flags]
    public enum SleepReason
    {
        NoLearnedAction = 1,
        Dead = 2,
        Power = 4,
    }
    public SleepReason sleepReason = SleepReason.NoLearnedAction | SleepReason.Dead;
    private readonly SleepReason sleepReasonMask = SleepReason.NoLearnedAction | SleepReason.Dead;

    /// <summary>
    /// 小火 UI：存活、无强制行动、且非 <see cref="IsBodyguardSleeping"/> 时，视为在用灯槽记录的意图（当前槽紫）；否则已学槽统一绿。
    /// </summary>
    public bool IsUsingTorchRecordedIntentForVisuals =>
        Creature.IsAlive && _forcedAction == null && !IsSleeping();

    /// <summary>
    /// 是否视为「沉睡」而不替主人承伤等（与灯槽展示一致）：任意非零 <see cref="sleepReason"/> 即视为睡，含仅 <see cref="SleepReason.NoLearnedAction"/>。
    /// </summary>
    public bool IsSleeping()
    {
        return sleepReason != 0;
    }

    /// <summary>
    /// 手牌打出时是否阻断聚合体「直接对敌」攻击：仅含 <see cref="SleepReason.NoLearnedAction"/>（无已学意图）时不阻断；含死亡、能力沉睡等则阻断。
    /// </summary>
    public bool BlockActionFromSleep => (sleepReason & ~SleepReason.NoLearnedAction) != 0;

    public async Task FallAsleep(SleepReason reason)
    {
        if (_forcedAction == null)
        {
            await BeginForcedAction(new AmalgamEmergencySleepForcedActionModel(0m));
        }

        bool wasSleeping = IsSleeping();
        sleepReason |= reason;
        if (IsSleeping())
        {
            await CreatureCmd.TriggerAnim(Creature, "Sleep", 0f);
            if (!wasSleeping && Creature.CombatState is { } combatState)
            {
                await FriendlyAmalgamHook.AfterFallAsleep(combatState, Creature);
            }
        }
    }

    public async Task WakeUp(SleepReason reason)
    {
        bool wasSleeping = IsSleeping();
        sleepReason &= ~reason;
        if (wasSleeping && !IsSleeping())
        {
            ClearForcedAction();
            await CreatureCmd.TriggerAnim(Creature, "Idle", 0f);
            if (Creature.CombatState is { } combatState)
            {
                await FriendlyAmalgamHook.AfterAwake(combatState, Creature);
            }
        }
    }
    #endregion

    /// <summary>进入强制行动；下回合由 <see cref="EmergencyEvasionPendingPower"/> 等逻辑调用 <see cref="ClearForcedAction"/>。</summary>
    public async Task BeginForcedAction(AmalgamForcedActionModel? action)
    {
        if (action == null)
        {
            return;
        }

        // 如果当前有沉睡强制行动，则不进入新的强制行动
        if (_forcedAction != null && _forcedAction.IsSleepingAction)
        {
            return;
        }

        _forcedAction = action;
        RefreshDisplayedIntent();
        FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(Creature);
        await _forcedAction.OnBeginAsync(Creature);
    }

    public void ClearForcedAction()
    {
        if (_forcedAction != null && _forcedAction.IsSleepingAction)
        {
            // 清除沉睡强制行动时，清除沉睡原因
            sleepReason &= sleepReasonMask;
        }

        _forcedAction = null;
        RefreshDisplayedIntent();
        FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(Creature);
    }

    #region 意图
    /// <summary>
    /// 获取当前要展示的意图状态
    /// </summary>
    private MoveState GetPendingDisplayedMoveState(Creature self)
    {
        // 紧急避险等跳过回合末灯槽执行的强制行动，使用强制行动的展示状态
        if (_forcedAction != null && _forcedAction.SkipsPlayerTurnEndTorchExecution)
        {
            return _forcedAction.MoveState;
        }

        // 终焉形态：合并多动意图条
        if (!IsSleeping() &&
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

    //TODO 对非ClearAfterExecute的处理要考虑进去
    /// <summary>
    /// 按 <see cref="BeforeTurnEnd"/> 相同轮数与灯槽轮转，把每一动要展示的 <see cref="AbstractIntent"/> 拼进 <paramref name="buffer"/>。
    /// </summary>
    private void BuildTerminusDisplayIntentScratch(Creature self, List<AbstractIntent> buffer, int totalPasses)
    {
        buffer.Clear();

        // 首先执行强制行动
        if (_forcedAction != null && !_forcedAction.SkipsPlayerTurnEndTorchExecution)
        {
            foreach (AbstractIntent intent in _forcedAction.GetMoveStateForDisplay(self).Intents)
            {
                buffer.Add(intent);
            }
            totalPasses--;
        }

        int idx = _currentTorchSlotIndex;
        for (int pass = 0; pass < totalPasses; pass++)
        {
            AmalgamActionModel? action = _intentByTorchSlot[idx];
            if (action == null)
            {
                break;
            }

            MoveState moveState = action.GetMoveStateForDisplay(self);
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
    #endregion

    public AmalgamActionModel? GetIntentInTorchSlot(int slotIndex) =>
        slotIndex >= 0 && slotIndex < TorchSlotCount ? _intentByTorchSlot[slotIndex] : null;
    private List<IHoverTip> _hoverTips = new();

    /// <summary>灯槽悬停：与意图节点悬停同源，按槽取 <see cref="AmalgamActionModel.GetMoveStateForDisplay"/> 并返回全部 <see cref="AbstractIntent.GetHoverTip"/>。</summary>
    public bool TryGetTorchSlotHoverTip(Creature amalgamCreature, int slotIndex, out IEnumerable<IHoverTip> hoverTips)
    {
        hoverTips = Array.Empty<IHoverTip>();
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
        _hoverTips.Clear();
        int intetIndex = 0;
        foreach (AbstractIntent intent in moveState.Intents)
        {
            HoverTip hoverTip = intent.GetHoverTip(targets, amalgamCreature);
            if (action is AmalgamCompositeIntentAction compositeAction)
            {
                LocString titleLoc = new("intents", QueenKeyword.GetCompositeIntentTitleLocKey(compositeAction.CompositeKey));
                titleLoc.Add("Title", hoverTip.Title ?? string.Empty);
                hoverTip = new HoverTip(titleLoc, hoverTip.Description, hoverTip.Icon);
            }
            hoverTip.Id += $"_{intetIndex++}";
            _hoverTips.Add(hoverTip);
        }

        hoverTips = _hoverTips;
        return true;
    }

    public override int MaxInitialHp => 0;
    public override int MinInitialHp => 0;

    /// <summary>友方聚合体始终显示血条（含 0 血、击倒沉睡待复活），便于读血与复苏；与奥斯提「尸体隐藏血条」刻意不同。</summary>
    public override bool IsHealthBarVisible => true;

    protected override string VisualsPath => "res://TheQueen/scenes/creature_visuals/torch_head_amalgam_minion.tscn";

    public override bool ShouldPowerBeRemovedOnDeath(PowerModel power)
    {
        if (power.Owner == this.Creature)
            return false;
        return true;
    }

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

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        await base.AfterTurnEnd(choiceContext, side);
        if (side != CombatSide.Player)
        {
            return;
        }
        await AfterTurnEndInternalAsync(choiceContext, side);
        await FriendlyAmalgamHook.AfterAmalgamTurnEnd(Creature.CombatState, Creature);
    }

    private async Task AfterTurnEndInternalAsync(PlayerChoiceContext choiceContext, CombatSide side)
    {
        Creature self = Creature;
        if (side != CombatSide.Player)
        {
            return;
        }

        // 击倒沉睡占一回合：先结算沉睡行动（1 血、无治疗演出），本回合末不执行灯槽意图。
        if (!self.IsAlive && self.GetPower<AmalgamDieForYouPower>() is { } deathSleepPower &&
            deathSleepPower.IsAwaitingDeathSleepRevive)
        {
            await FriendlyAmalgamCmd.TryPerformIntent(self);
            await deathSleepPower.ExecuteDeathSleepReviveSilentlyAsync(choiceContext, self);
            return;
        }

        if (!self.IsAlive)
        {
            await FriendlyAmalgamCmd.TryPerformIntent(self);
            return;
        }

        // 计算行动次数
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

        // 执行强制行动，占一次行动
        if (_forcedAction != null)
        {
            await FriendlyAmalgamCmd.TryPerformIntent(self);
            await _forcedAction.ExecuteAsync(choiceContext, self);
            if (_forcedAction.ClearAfterExecute)
            {
                ClearForcedAction();
            }
            if (_forcedAction.SkipsPlayerTurnEndTorchExecution)
            {
                RefreshDisplayedIntent();
                FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(self);
                return;
            }
            endTurnPasses--;
        }


        for (int pass = 0; pass < endTurnPasses; pass++)
        {
            SyncCurrentTorchSlotIfNeeded();
            AmalgamActionModel? action = LearnedAction;
            if (action == null)
            {
                break;
            }
            if (BlockActionFromSleep)
            {
                break;
            }

            await FriendlyAmalgamCmd.TryPerformIntent(self);
            await action.ExecuteAsync(choiceContext, self);
            RotateCurrentToNextLitTorchSlot();
            FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(self);
        }
        
        // 行动结束，刷新意图展示
        RefreshDisplayedIntent();
    }

    /// <summary>立刻执行当前灯槽记录的意图（意图条演出 + 结算），<strong>不</strong>清空槽位。</summary>
    public async Task ActCurrentIntentImmediatelyAsync(PlayerChoiceContext choiceContext, bool skipRotate = false)
    {
        Creature self = Creature;
        if (!self.IsAlive)
        {
            return;
        }

        SyncCurrentTorchSlotIfNeeded();

        AmalgamActionModel? action = _forcedAction != null ? _forcedAction : LearnedAction;
        if (action == null)
        {
            return;
        }

        await FriendlyAmalgamCmd.TryPerformIntent(self);

        if (IsSleeping())
        {
            return;
        }

        await action.ExecuteAsync(choiceContext, self);
        if (action == _forcedAction && _forcedAction?.ClearAfterExecute == true)
        {
            ClearForcedAction();
        }
        else if (!skipRotate)
        {
            RotateCurrentToNextLitTorchSlot();
        }
        RefreshDisplayedIntent();
        FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(self);

        // 立刻行动可能在 Power hook / 非 GameAction 上下文里斩杀敌人，需主动判胜。
        await CombatManager.Instance.CheckWinCondition();
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

        if (_forcedAction != null)
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
            await FallAsleep(SleepReason.NoLearnedAction);
        }

        RefreshDisplayedIntent();
        FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(self);
    }

    /// <summary>移除所有非 <see cref="AmalgamCompositeIntentAction"/> 灯槽意图，返回其克隆（按槽位 0→2 顺序）。</summary>
    public async Task<IReadOnlyList<AmalgamActionModel>> ForgetAllNonCompositeTorchIntentsAsync()
    {
        Creature self = Creature;
        if (!self.IsAlive)
        {
            return [];
        }

        List<AmalgamActionModel> forgotten = [];
        for (int i = 0; i < TorchSlotCount; i++)
        {
            AmalgamActionModel? action = _intentByTorchSlot[i];
            if (action is null or AmalgamCompositeIntentAction)
            {
                continue;
            }

            forgotten.Add(action.Clone());
            _intentByTorchSlot[i] = null;
        }

        if (forgotten.Count == 0)
        {
            return forgotten;
        }

        bool anyIntentLeft = false;
        for (int i = 0; i < TorchSlotCount; i++)
        {
            if (_intentByTorchSlot[i] != null)
            {
                anyIntentLeft = true;
                break;
            }
        }

        if (!anyIntentLeft)
        {
            _currentTorchSlotIndex = 0;
            await FallAsleep(SleepReason.NoLearnedAction);
        }
        else
        {
            SyncCurrentTorchSlotIfNeeded();
        }

        RefreshDisplayedIntent();
        FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(self);
        return forgotten;
    }

    /// <returns>是否写入灯槽；三槽已满当场执行时返回 <see langword="false"/>。</returns>
    public async Task<bool> LearnIntent(PlayerChoiceContext choiceContext, AmalgamActionModel intent)
    {
        int emptySlot = FirstEmptyTorchSlotIndex();
        if (emptySlot < 0)
        {
            // 三槽已满：不写入槽位，当场执行本次要学的意图；不做意图条/小火等意图 UI 同步。
            // 能力/死亡等沉睡（BlockActionFromSleep）时与回合末一致，不执行。
            if (!BlockActionFromSleep)
            {
                await intent.ExecuteAsync(choiceContext, Creature);
            }
            return false;
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
        if (!hadAnyIntentBefore)
        {
            await WakeUp(SleepReason.NoLearnedAction);
        }
        FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(Creature);
        return true;
    }

    /// <summary>
    /// 按 <paramref name="compositeKey"/> 合并意图：若某灯槽已有同键的 <see cref="AmalgamCompositeIntentAction"/>，则把 <paramref name="intent"/> 追加到该条组合内；
    /// 否则无空槽时走 <see cref="LearnIntent"/>（三槽满时与单次学习相同：当场执行且不写入）；
    /// 有空槽则新建一条组合意图并 <see cref="LearnIntent"/>。
    /// </summary>
    /// <returns>是否写入灯槽或合并进已有组合；三槽已满当场执行时返回 <see langword="false"/>。</returns>
    public async Task<bool> CombineIntentAsync(PlayerChoiceContext choiceContext, AmalgamActionModel intent, AmalgamCompositeKey compositeKey)
    {
        for (int i = 0; i < TorchSlotCount; i++)
        {
            if (_intentByTorchSlot[i] is AmalgamCompositeIntentAction composite &&
                composite.CompositeKey == compositeKey)
            {
                composite.AddPart(intent);
                RefreshDisplayedIntent();
                FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(Creature);
                return true;
            }
        }

        if (FirstEmptyTorchSlotIndex() < 0)
        {
            return await LearnIntent(choiceContext, intent);
        }

        AmalgamCompositeIntentAction bundle = new(compositeKey, intent);
        return await LearnIntent(choiceContext, bundle);
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
        await FallAsleep(SleepReason.NoLearnedAction);
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
        if (self.CombatState == null || MoveStateMachine == null)
        {
            return;
        }

        MoveState state = GetPendingDisplayedMoveState(self);
        SetMoveImmediate(state, forceTransition: true);
    }
}