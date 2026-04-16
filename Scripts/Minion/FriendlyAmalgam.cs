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

    public AmalgamActionModel? LearnedAction { get; private set; }

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

        // 使用你定义的 AnimName 作为 trigger，调用 TriggerAnim 时可直接传这些常量。
        animator.AddAnyState(IdleAnimName, idle);
        animator.AddAnyState(AttackAnimName, attack);
        animator.AddAnyState(BuffAnimName, buff);
        animator.AddAnyState(DebuffAnimName, debuff);
        animator.AddAnyState(PowerAttackAnimName, powerAttack);
        animator.AddAnyState(SleepAnimName, sleep);
        animator.AddAnyState(DeathAnimName, death);

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

    public override Task OnSummon(Player owner, Creature self, MinionSummonOptions options)
    {
        // TODO 加上为你而死，初始化意图效果
        LearnedAction = null;
        RefreshDisplayedIntent();
        return CreatureCmd.TriggerAnim(self, SleepAnimName, 0f);
    }

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        await base.BeforeTurnEnd(choiceContext, side);

        Creature self = Creature;
        if (side != CombatSide.Player || !self.IsAlive)
        {
            return;
        }

        AmalgamActionModel? action = LearnedAction;
        if (action == null)
        {
            return;
        }

        await action.ExecuteAsync(choiceContext, self);
        RefreshDisplayedIntent();
    }

    public async Task LearnIntent(AmalgamActionModel intent)
    {
        LearnedAction = intent;
        RefreshDisplayedIntent();
        await CreatureCmd.TriggerAnim(Creature, IdleAnimName, 0f);
    }

    private void RefreshDisplayedIntent()
    {
        MoveState state = LearnedAction?.MoveState ?? DefaultSleepMoveState;
        SetMoveImmediate(state, forceTransition: true);
    }
}