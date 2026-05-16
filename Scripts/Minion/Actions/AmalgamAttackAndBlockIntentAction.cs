using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：先攻击，再为玩家获得格挡。</summary>
public sealed class AmalgamAttackAndBlockIntentAction : AmalgamActionModel
{
    private const string BlockParam = "block";

    private readonly decimal _block;

    public AmalgamAttackAndBlockIntentAction(decimal damage, decimal block)
        : base(new Dictionary<string, decimal>
        {
            [AmountParam] = damage,
            [BlockParam] = block,
        })
    {
        _block = GetParameterOrDefault(BlockParam, 0m);
    }

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_ATTACK_AND_BLOCK",
            _ => Task.CompletedTask,
            new AmalgamSingleAttackIntent(Amount),
            new AmalgamGainBlockIntent(_block));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        if (Amount > 0m)
        {
            await ExecuteOffensePart(choiceContext, amalgam);
        }

        if (_block > 0m
            && amalgam.PetOwner is { Creature: { } owner }
            && owner.IsAlive)
        {
            await CreatureCmd.TriggerAnim(amalgam, "Cast", AmalgamGainBlockIntentAction.CastAnimDelay);
            await CreatureCmd.GainBlock(owner, _block, ValueProp.Move, null);
        }
    }

    private async Task ExecuteOffensePart(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        ICombatState? combatState = amalgam.CombatState;
        if (combatState == null || amalgam.PetOwner is not Player queen)
        {
            return;
        }

        Creature[] alive = combatState.Enemies.Where(e => e.IsAlive).ToArray();
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
}
