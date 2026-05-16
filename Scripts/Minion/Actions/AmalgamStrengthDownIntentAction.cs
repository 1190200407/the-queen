using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：按 <see cref="AmalgamOffenseTargeting"/> 使敌人本回合失去力量（回合结束恢复）。</summary>
public sealed class AmalgamStrengthDownIntentAction : AmalgamActionModel
{
    private readonly Creature? _forcedTarget;

    public AmalgamStrengthDownIntentAction(decimal strengthLoss)
        : base(strengthLoss)
    {
    }

    public AmalgamStrengthDownIntentAction(decimal strengthLoss, Creature? forcedTarget)
        : base(strengthLoss)
    {
        _forcedTarget = forcedTarget;
    }

    public static readonly float CastAnimDelay = 1.5f;

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_STRENGTH_DOWN",
            _ => Task.CompletedTask,
            new AmalgamStrengthDownIntent(Amount));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        ICombatState? combatState = amalgam.CombatState;
        if (combatState == null || amalgam.PetOwner is not Player queen || !queen.Creature.IsAlive || Amount <= 0m)
        {
            return;
        }

        Creature[] alive = combatState.Enemies.Where(e => e.IsAlive).ToArray();
        if (alive.Length == 0)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Cast", CastAnimDelay);

        AmalgamOffenseTargetingMode mode = AmalgamOffenseTargeting.ResolveMode(combatState, queen);
        Creature applier = queen.Creature;

        if (_forcedTarget is { IsAlive: true } forcedTarget && alive.Contains(forcedTarget))
        {
            await PowerCmd.Apply<AmalgamIntentStrengthDownPower>(choiceContext, forcedTarget, Amount, applier, null);
            return;
        }

        if (mode == AmalgamOffenseTargetingMode.AllAliveEnemies)
        {
            foreach (Creature enemy in alive)
            {
                await PowerCmd.Apply<AmalgamIntentStrengthDownPower>(choiceContext, enemy, Amount, applier, null);
            }

            return;
        }

        if (mode == AmalgamOffenseTargetingMode.LockedMarkedEnemy)
        {
            Creature? marked = AmalgamOffenseTargeting.FindMarkedEnemy(combatState);
            if (marked is { IsAlive: true })
            {
                await PowerCmd.Apply<AmalgamIntentStrengthDownPower>(choiceContext, marked, Amount, applier, null);
            }

            return;
        }

        Creature? randomEnemy = FriendlyAmalgamCmd.NextRandomHittableEnemy(queen, alive);
        if (randomEnemy is not { IsAlive: true })
        {
            return;
        }

        await PowerCmd.Apply<AmalgamIntentStrengthDownPower>(choiceContext, randomEnemy, Amount, applier, null);
    }
}

