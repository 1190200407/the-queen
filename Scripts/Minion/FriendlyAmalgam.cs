using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MinionLib.Minion;

namespace ComicChess.TheQueen;

public class FriendlyAmalgam : MinionModel
{
    private static readonly MoveState DefaultSleepMoveState = new(
        "AMALGAM_SLEEP",
        _ => Task.CompletedTask,
        new SleepIntent());

    private const int TorchSlotCount = 3;

    /// <summary>每个灯槽一条已学意图；按槽 0→1→2 顺序点亮空槽，意图不随执行而清除。</summary>
    private readonly AmalgamActionModel?[] _intentByTorchSlot = new AmalgamActionModel?[TorchSlotCount];

    /// <summary>当前轮到执行的灯槽下标（仅在已点亮的槽之间轮转）。</summary>
    private int _currentTorchSlotIndex;

    /// <summary>当前灯槽对应的意图（用于展示与 <see cref="BeforeTurnEnd"/> 执行）。</summary>
    public AmalgamActionModel? LearnedAction => _intentByTorchSlot[_currentTorchSlotIndex];

    public bool HasIntentInTorchSlot(int slotIndex) =>
        slotIndex >= 0 && slotIndex < TorchSlotCount && _intentByTorchSlot[slotIndex] != null;

    public override int MaxInitialHp => 1;
    public override int MinInitialHp => 1;

    protected override string VisualsPath => "res://TheQueen/scenes/creature_visuals/torch_head_amalgam_minion.tscn";

    public static readonly string IdleAnimName = "idle_loop";
    public static readonly string DeathAnimName = "die";
    public static readonly string BuffAnimName = "buff";
    public static readonly string DebuffAnimName = "_ignore/hug2";
    public static readonly string AttackAnimName = "attack";
    public static readonly string PowerAttackAnimName = "debuff";
    public static readonly string SleepAnimName = "_ignore/string_rigging";

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new(IdleAnimName, isLooping: true);
        AnimState attack = new(AttackAnimName);
        AnimState buff = new(BuffAnimName);
        AnimState debuff = new(DebuffAnimName);
        AnimState powerAttack = new(PowerAttackAnimName);
        AnimState sleep = new(SleepAnimName, isLooping: true);
        AnimState death = new(DeathAnimName);

        attack.NextState = idle;
        buff.NextState = idle;
        debuff.NextState = idle;
        powerAttack.NextState = idle;
        death.NextState = idle;

        CreatureAnimator animator = new(idle, controller);

        // 兼容引擎与常见调用习惯。
        animator.AddAnyState("Idle", idle);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Cast", buff);
        animator.AddAnyState("Hit", debuff);
        animator.AddAnyState("Dead", death);
        animator.AddAnyState("PowerAttack", powerAttack);
        animator.AddAnyState("Sleep", sleep);

        return animator;
    }

    public override async Task OnSummon(Player owner, Creature self, MinionSummonOptions options)
    {
        // TODO 加上为你而死，初始化意图效果
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

        AmalgamActionModel? action = LearnedAction;
        if (action == null)
        {
            return;
        }

        await action.ExecuteAsync(choiceContext, self);
        RotateCurrentToNextLitTorchSlot();
        RefreshDisplayedIntent();
    }

    public async Task LearnIntent(AmalgamActionModel intent)
    {
        int emptySlot = FirstEmptyTorchSlotIndex();
        if (emptySlot < 0)
        {
            // 三槽已满时的学习规则待定，暂不写入。
            return;
        }

        _intentByTorchSlot[emptySlot] = intent;
        if (_intentByTorchSlot[_currentTorchSlotIndex] == null)
        {
            _currentTorchSlotIndex = IndexOfFirstLitTorchSlot();
        }

        RefreshDisplayedIntent();
        FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(Creature);
        await CreatureCmd.TriggerAnim(Creature, "Idle", 0f);
    }

    private async Task ClearTorchSlots()
    {
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

    private int IndexOfFirstLitTorchSlot()
    {
        for (int i = 0; i < TorchSlotCount; i++)
        {
            if (_intentByTorchSlot[i] != null)
            {
                return i;
            }
        }

        return 0;
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
        MoveState state = LearnedAction?.MoveState ?? DefaultSleepMoveState;
        SetMoveImmediate(state, forceTransition: true);
    }
}