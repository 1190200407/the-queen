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

/// <summary>聚合体意图：�?<see cref="AmalgamOffenseTargeting"/> 对敌人施�?<see cref="AmalgamActionModel.Amount"/> 层易伤�?/summary>
public sealed class AmalgamApplyVulnerableIntentAction : AmalgamActionModel
{
    public override string Key => "vulnerable";

    private Creature? _forcedTarget;

    public AmalgamApplyVulnerableIntentAction()
    {
    }

    public AmalgamApplyVulnerableIntentAction(decimal vulnerableStacks)
        : base(vulnerableStacks)
    {
    }

    public AmalgamApplyVulnerableIntentAction(decimal vulnerableStacks, Creature? forcedTarget)
        : this(vulnerableStacks)
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
            "AMALGAM_INTENT_VULNERABLE",
            _ => Task.CompletedTask,
            new AmalgamApplyVulnerableIntent(Amount));
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
            await PowerCmd.Apply<VulnerablePower>(choiceContext, forcedTarget, Amount, applier, null);
            return;
        }

        if (mode == AmalgamOffenseTargetingMode.AllAliveEnemies)
        {
            foreach (Creature enemy in alive)
            {
                await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, Amount, applier, null);
            }

            return;
        }

        if (mode == AmalgamOffenseTargetingMode.LockedMarkedEnemy)
        {
            Creature? marked = AmalgamOffenseTargeting.FindMarkedEnemy(combatState);
            if (marked is { IsAlive: true })
            {
                await PowerCmd.Apply<VulnerablePower>(choiceContext, marked, Amount, applier, null);
            }

            return;
        }

        Creature? randomEnemy = FriendlyAmalgamCmd.NextRandomHittableEnemy(queen, alive);
        if (randomEnemy is not { IsAlive: true })
        {
            return;
        }

        await PowerCmd.Apply<VulnerablePower>(choiceContext, randomEnemy, Amount, applier, null);
    }
}
