using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MinionLib.Minion;

namespace ComicChess.TheQueen;

public class FriendlyAmalgam : MinionModel
{
    /// <summary>与默认沉睡、<see cref="AmalgamEmergencySleepForcedActionModel"/> 共用同一展示状态。</summary>
    internal static readonly MoveState SleepOverlayMoveState = new(
        "AMALGAM_SLEEP",
        _ => Task.CompletedTask,
        new AmalgamSleepIntent());

    private const int TorchSlotCount = 3;

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
    /// 是否视为「沉睡」而不替主人承伤：以<strong>即将执行的 MoveState</strong>为准（首意图为 <see cref="AmalgamSleepIntent"/>），
    /// 或存在 <see cref="AmalgamForcedActionModel"/> 且 <see cref="AmalgamForcedActionModel.IsSleepingForBodyguard"/> 为真（如 <see cref="AmalgamEmergencySleepForcedActionModel"/>）；不单看灯槽是否学满进攻。
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

        return IsSleepPendingMoveState(GetPendingDisplayedMoveState(self));
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

        if (LearnedAction != null)
        {
            return LearnedAction.GetMoveStateForDisplay(self);
        }

        return SleepOverlayMoveState;
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

        AbstractIntent firstIntent = moveState.Intents[0];
        IEnumerable<Creature> targets = combatState.Players.Select(static p => p.Creature);
        hoverTip = firstIntent.GetHoverTip(targets, amalgamCreature);
        return true;
    }

    public override int MaxInitialHp => 1;
    public override int MinInitialHp => 1;

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
        if (side != CombatSide.Player || !self.IsAlive)
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

        AmalgamActionModel? action = LearnedAction;
        if (action == null)
        {
            return;
        }

        if (IsSleepPendingMoveState(action.GetMoveStateForDisplay(self)))
        {
            RefreshDisplayedIntent();
            FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(self);
            return;
        }

        await FriendlyAmalgamCmd.TryPerformIntent(self);
        await action.ExecuteAsync(choiceContext, self);
        RotateCurrentToNextLitTorchSlot();
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
        if (_intentByTorchSlot[_currentTorchSlotIndex] == null)
        {
            return;
        }

        for (int step = 1; step <= TorchSlotCount; step++)
        {
            int idx = (_currentTorchSlotIndex + step) % TorchSlotCount;
            if (_intentByTorchSlot[idx] != null)
            {
                _currentTorchSlotIndex = idx;
                return;
            }
        }
    }

    private void RefreshDisplayedIntent()
    {
        Creature self = Creature;
        MoveState state = GetPendingDisplayedMoveState(self);
        SetMoveImmediate(state, forceTransition: true);
    }
}