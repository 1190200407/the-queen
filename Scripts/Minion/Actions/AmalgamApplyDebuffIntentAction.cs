using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>通用聚合体 Debuff 意图：对敌人施加若干层某种 Debuff（Power）。</summary>
public sealed class AmalgamApplyDebuffIntentAction<T> : AmalgamActionModel
    where T : PowerModel
{
    private readonly Creature? _forcedTarget;
    private readonly string _debuffEntryId;

    public AmalgamApplyDebuffIntentAction(decimal stacks, string debuffEntryId)
        : base(stacks)
    {
        _debuffEntryId = debuffEntryId;
    }

    public AmalgamApplyDebuffIntentAction(decimal stacks, string debuffEntryId, Creature? forcedTarget)
        : this(stacks, debuffEntryId)
    {
        _forcedTarget = forcedTarget;
    }

    public static readonly float CastAnimDelay = 1.5f;

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_APPLY_DEBUFF",
            _ => Task.CompletedTask,
            new AmalgamApplyDebuffIntent(_debuffEntryId, Amount));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        ICombatState? combatState = amalgam.CombatState;
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
            await PowerCmd.Apply<T>(choiceContext, forcedTarget, Amount, applier, null);
            return;
        }

        if (mode == AmalgamOffenseTargetingMode.AllAliveEnemies)
        {
            foreach (Creature enemy in alive)
            {
                await PowerCmd.Apply<T>(choiceContext, enemy, Amount, applier, null);
            }

            return;
        }

        if (mode == AmalgamOffenseTargetingMode.LockedMarkedEnemy)
        {
            Creature? marked = AmalgamOffenseTargeting.FindMarkedEnemy(combatState);
            if (marked is { IsAlive: true })
            {
                await PowerCmd.Apply<T>(choiceContext, marked, Amount, applier, null);
            }

            return;
        }

        Creature? randomEnemy = FriendlyAmalgamCmd.NextRandomHittableEnemy(queen, alive);
        if (randomEnemy is not { IsAlive: true })
        {
            return;
        }

        await PowerCmd.Apply<T>(choiceContext, randomEnemy, Amount, applier, null);
    }
}

