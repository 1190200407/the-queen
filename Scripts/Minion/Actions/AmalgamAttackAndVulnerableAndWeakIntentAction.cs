using System.Collections.Generic;
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
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：先攻击，再施加易伤与虚弱（同一个 action，包含三个 intents）。</summary>
public sealed class AmalgamAttackAndVulnerableAndWeakIntentAction : AmalgamActionModel
{
    private const string VulnerableParam = "vulnerable";
    private const string WeakParam = "weak";

    private readonly decimal _vulnerable;
    private readonly decimal _weak;

    public AmalgamAttackAndVulnerableAndWeakIntentAction(decimal damage, decimal vulnerable, decimal weak)
        : base(new Dictionary<string, decimal>
        {
            [AmountParam] = damage,
            [VulnerableParam] = vulnerable,
            [WeakParam] = weak,
        })
    {
        _vulnerable = GetParameterOrDefault(VulnerableParam, 0m);
        _weak = GetParameterOrDefault(WeakParam, 0m);
    }

    protected override MoveState CreateMoveState()
    {
        // Note: intent label/description is handled by the intents themselves.
        return new MoveState(
            "AMALGAM_INTENT_ATTACK_AND_VULNERABLE_AND_WEAK",
            _ => Task.CompletedTask,
            new AmalgamSingleAttackIntent(Amount),
            new AmalgamApplyVulnerableIntent(_vulnerable),
            new AmalgamApplyWeakIntent(_weak));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        if (Amount > 0m)
        {
            await ExecuteOffensePart(choiceContext, amalgam);
        }

        if (_vulnerable > 0m)
        {
            await ExecuteVulnerablePart(amalgam);
        }

        if (_weak > 0m)
        {
            await ExecuteWeakPart(amalgam);
        }
    }

    private static Creature[] GetAliveEnemies(ICombatState combatState) => combatState.Enemies.Where(e => e.IsAlive).ToArray();

    private async Task ExecuteOffensePart(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        ICombatState? combatState = amalgam.CombatState;
        if (combatState == null || amalgam.PetOwner is not Player queen)
        {
            return;
        }

        Creature[] alive = GetAliveEnemies(combatState);
        if (alive.Length == 0)
        {
            return;
        }

        AmalgamOffenseTargetingMode mode = AmalgamOffenseTargeting.ResolveMode(combatState, queen);
        if (mode == AmalgamOffenseTargetingMode.AllAliveEnemies)
        {
            await FriendlyAmalgamCmd.ExecuteAllEnemiesSingleSwingAttack(
                choiceContext,
                amalgam,
                alive,
                Amount,
                "Attack",
                0.6f,
                "vfx/vfx_attack_blunt");
            return;
        }

        if (mode == AmalgamOffenseTargetingMode.LockedMarkedEnemy)
        {
            Creature? marked = AmalgamOffenseTargeting.FindMarkedEnemy(combatState);
            if (marked is { IsAlive: true })
            {
                await FriendlyAmalgamCmd.ExecuteSingleTargetAttack(
                    choiceContext,
                    amalgam,
                    marked,
                    Amount,
                    "Attack",
                    0.6f,
                    "vfx/vfx_attack_blunt");
                return;
            }
        }

        Creature? randomEnemy = FriendlyAmalgamCmd.NextRandomHittableEnemy(queen, alive);
        if (randomEnemy is not { IsAlive: true })
        {
            return;
        }

        await FriendlyAmalgamCmd.ExecuteSingleTargetAttack(
            choiceContext,
            amalgam,
            randomEnemy,
            Amount,
            "Attack",
            0.6f,
            "vfx/vfx_attack_blunt");
    }

    private async Task ExecuteVulnerablePart(Creature amalgam)
    {
        ICombatState? combatState = amalgam.CombatState;
        if (combatState == null || amalgam.PetOwner is not Player queen || !queen.Creature.IsAlive)
        {
            return;
        }

        Creature[] alive = GetAliveEnemies(combatState);
        if (alive.Length == 0)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Cast", AmalgamApplyVulnerableIntentAction.CastAnimDelay);

        AmalgamOffenseTargetingMode mode = AmalgamOffenseTargeting.ResolveMode(combatState, queen);
        Creature applier = queen.Creature;

        if (mode == AmalgamOffenseTargetingMode.AllAliveEnemies)
        {
            foreach (Creature enemy in alive)
            {
                await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, _vulnerable, applier, null);
            }

            return;
        }

        if (mode == AmalgamOffenseTargetingMode.LockedMarkedEnemy)
        {
            Creature? marked = AmalgamOffenseTargeting.FindMarkedEnemy(combatState);
            if (marked is { IsAlive: true })
            {
                await PowerCmd.Apply<VulnerablePower>(choiceContext, marked, _vulnerable, applier, null);
            }

            return;
        }

        Creature? randomEnemy = FriendlyAmalgamCmd.NextRandomHittableEnemy(queen, alive);
        if (randomEnemy is not { IsAlive: true })
        {
            return;
        }

        await PowerCmd.Apply<VulnerablePower>(choiceContext, randomEnemy, _vulnerable, applier, null);
    }

    private async Task ExecuteWeakPart(Creature amalgam)
    {
        ICombatState? combatState = amalgam.CombatState;
        if (combatState == null || amalgam.PetOwner is not Player queen || !queen.Creature.IsAlive)
        {
            return;
        }

        Creature[] alive = GetAliveEnemies(combatState);
        if (alive.Length == 0)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Cast", AmalgamApplyWeakIntentAction.CastAnimDelay);

        AmalgamOffenseTargetingMode mode = AmalgamOffenseTargeting.ResolveMode(combatState, queen);
        Creature applier = queen.Creature;

        if (mode == AmalgamOffenseTargetingMode.AllAliveEnemies)
        {
            foreach (Creature enemy in alive)
            {
                await PowerCmd.Apply<WeakPower>(choiceContext, enemy, _weak, applier, null);
            }

            return;
        }

        if (mode == AmalgamOffenseTargetingMode.LockedMarkedEnemy)
        {
            Creature? marked = AmalgamOffenseTargeting.FindMarkedEnemy(combatState);
            if (marked is { IsAlive: true })
            {
                await PowerCmd.Apply<WeakPower>(choiceContext, marked, _weak, applier, null);
            }

            return;
        }

        Creature? randomEnemy = FriendlyAmalgamCmd.NextRandomHittableEnemy(queen, alive);
        if (randomEnemy is not { IsAlive: true })
        {
            return;
        }

        await PowerCmd.Apply<WeakPower>(choiceContext, randomEnemy, _weak, applier, null);
    }
}

