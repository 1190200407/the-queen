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

/// <summary>聚合体意图：按 <see cref="AmalgamOffenseTargeting"/> 对敌人施加 <see cref="AmalgamActionModel.Amount"/> 层易伤。</summary>
public sealed class AmalgamApplyVulnerableIntentAction : AmalgamActionModel
{
    private readonly Creature? _forcedTarget;

    public AmalgamApplyVulnerableIntentAction(decimal vulnerableStacks)
        : base(vulnerableStacks)
    {
    }

    public AmalgamApplyVulnerableIntentAction(decimal vulnerableStacks, Creature? forcedTarget)
        : base(vulnerableStacks)
    {
        _forcedTarget = forcedTarget;
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
            await PowerCmd.Apply<VulnerablePower>(forcedTarget, Amount, applier, null);
            return;
        }

        if (mode == AmalgamOffenseTargetingMode.AllAliveEnemies)
        {
            foreach (Creature enemy in alive)
            {
                await PowerCmd.Apply<VulnerablePower>(enemy, Amount, applier, null);
            }

            return;
        }

        if (mode == AmalgamOffenseTargetingMode.LockedMarkedEnemy)
        {
            Creature? marked = AmalgamOffenseTargeting.FindMarkedEnemy(combatState);
            if (marked is { IsAlive: true })
            {
                await PowerCmd.Apply<VulnerablePower>(marked, Amount, applier, null);
            }

            return;
        }

        Creature randomEnemy = alive[System.Random.Shared.Next(alive.Length)];
        await PowerCmd.Apply<VulnerablePower>(randomEnemy, Amount, applier, null);
    }
}
