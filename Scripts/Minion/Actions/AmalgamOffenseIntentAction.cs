using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>
/// 聚合体意图：造成 <see cref="AmalgamActionModel.Amount"/> 点伤害；选敌由 <see cref="AmalgamOffenseTargetingMode"/> 决定。
/// </summary>
public sealed class AmalgamOffenseIntentAction : AmalgamActionModel
{
    private readonly Creature? _forcedTarget;

    public AmalgamOffenseIntentAction(decimal damage)
        : base(damage)
    {
    }

    public AmalgamOffenseIntentAction(decimal damage, Creature? forcedTarget)
        : this(damage)
    {
        _forcedTarget = forcedTarget;
    }

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_OFFENSE",
            _ => Task.CompletedTask,
            new AmalgamSingleAttackIntent(Amount));
    }

    /// <remarks>意图条 <c>PerformIntent</c> 仅在回合末执行灯槽记录时由 <see cref="FriendlyAmalgam.BeforeTurnEnd"/> 调用；此处只打出伤害链。</remarks>
    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        CombatState? combatState = amalgam.CombatState;
        if (combatState == null || amalgam.PetOwner is not Player queen)
        {
            return;
        }

        Creature[] alive = combatState.Enemies.Where(e => e.IsAlive).ToArray();
        if (alive.Length == 0)
        {
            return;
        }

        if (_forcedTarget is { IsAlive: true } forcedTarget && alive.Contains(forcedTarget))
        {
            await FriendlyAmalgamCmd.ExecuteSingleTargetAttack(
                choiceContext,
                amalgam,
                forcedTarget,
                Amount,
                "Attack",
                0.6f,
                "vfx/vfx_attack_blunt");
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

        Creature randomEnemy = alive[Random.Shared.Next(alive.Length)];
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

