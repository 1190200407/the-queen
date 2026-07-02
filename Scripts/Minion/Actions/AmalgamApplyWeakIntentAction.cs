using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：�?<see cref="AmalgamOffenseTargeting"/> 对敌人施�?<see cref="AmalgamActionModel.Amount"/> 层虚弱�?/summary>
public sealed class AmalgamApplyWeakIntentAction : AmalgamActionModel
{
    public override string Key => "weak";

    private Creature? _forcedTarget;

    public AmalgamApplyWeakIntentAction()
    {
    }

    public AmalgamApplyWeakIntentAction(decimal weakStacks)
        : base(weakStacks)
    {
    }

    public AmalgamApplyWeakIntentAction(decimal weakStacks, Creature? forcedTarget)
        : this(weakStacks)
    {
        _forcedTarget = forcedTarget;
    }

    public override bool Init(decimal amount)
    {
        if (!AmalgamActionArgs.IsPositive(amount))
        {
            return false;
        }

        ResetForInit();
        Amount = amount;
        _forcedTarget = null;
        return true;
    }

    public override bool Init(object[] args)
    {
        if (!AmalgamActionArgs.TryGetDecimal(args, 0, out decimal stacks) || !AmalgamActionArgs.IsPositive(stacks))
        {
            return false;
        }

        ResetForInit();
        Amount = stacks;
        _forcedTarget = AmalgamActionArgs.TryGetCreature(args, 1);
        return true;
    }

    public static readonly float CastAnimDelay = 1.5f;

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_WEAK",
            _ => Task.CompletedTask,
            new AmalgamApplyWeakIntent(Amount));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        CombatState? combatState = amalgam.CombatState;
        if (combatState == null || amalgam.PetOwner is not Player queen || !queen.Creature.IsAlive)
        {
            return;
        }

        Creature[] alive = combatState.Enemies.Where(e => e.IsAlive).ToArray();
        if (alive.Length == 0 || Amount <= 0m)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Cast", CastAnimDelay);

        AmalgamOffenseTargetingMode mode = AmalgamOffenseTargeting.ResolveMode(combatState, queen);
        Creature applier = queen.Creature;

        if (_forcedTarget is { IsAlive: true } forcedTarget && alive.Contains(forcedTarget))
        {
            await PowerCmd.Apply<WeakPower>(forcedTarget, Amount, applier, null);
            return;
        }

        if (mode == AmalgamOffenseTargetingMode.AllAliveEnemies)
        {
            foreach (Creature enemy in alive)
            {
                await PowerCmd.Apply<WeakPower>(enemy, Amount, applier, null);
            }

            return;
        }

        if (mode == AmalgamOffenseTargetingMode.LockedMarkedEnemy)
        {
            Creature? marked = AmalgamOffenseTargeting.FindMarkedEnemy(combatState);
            if (marked is { IsAlive: true })
            {
                await PowerCmd.Apply<WeakPower>(marked, Amount, applier, null);
            }

            return;
        }

        Creature? randomEnemy = FriendlyAmalgamCmd.NextRandomHittableEnemy(queen, alive);
        if (randomEnemy is not { IsAlive: true })
        {
            return;
        }

        await PowerCmd.Apply<WeakPower>(randomEnemy, Amount, applier, null);
    }
}

