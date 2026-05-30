using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

/// <summary>填装射击：下回合开始时，聚合体按选敌逻辑造成一次伤害，然后移除自身。</summary>
public sealed class AmalgamNextRoundAttackPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool IsInstanced => true;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (!base.Owner.IsAlive)
        {
            await PowerCmd.Remove(this);
            return;
        }

        if (side != base.Owner.Side || Amount <= 0m)
        {
            return;
        }

        if (base.Owner.PetOwner is not Player queen)
        {
            return;
        }

        Creature[] alive = combatState.Enemies.Where(e => e.IsAlive).ToArray();
        if (alive.Length == 0)
        {
            await PowerCmd.Remove(this);
            return;
        }

        AmalgamOffenseTargetingMode mode = AmalgamOffenseTargeting.ResolveMode(combatState, queen);
        if (mode == AmalgamOffenseTargetingMode.AllAliveEnemies)
        {
            await FriendlyAmalgamCmd.ExecuteAllEnemiesSingleSwingAttack(
                new ThrowingPlayerChoiceContext(),
                base.Owner,
                alive,
                Amount,
                "Attack",
                0.6f,
                "vfx/vfx_attack_blunt");
            await PowerCmd.Remove(this);
            return;
        }

        if (mode == AmalgamOffenseTargetingMode.LockedMarkedEnemy)
        {
            Creature? marked = AmalgamOffenseTargeting.FindMarkedEnemy(combatState);
            if (marked is { IsAlive: true })
            {
                await FriendlyAmalgamCmd.ExecuteSingleTargetAttack(
                    new ThrowingPlayerChoiceContext(),
                    base.Owner,
                    marked,
                    Amount,
                    "Attack",
                    0.6f,
                    "vfx/vfx_attack_blunt");
                await PowerCmd.Remove(this);
                return;
            }
        }

        Creature? randomEnemy = FriendlyAmalgamCmd.NextRandomHittableEnemy(queen, alive);
        if (randomEnemy is not { IsAlive: true })
        {
            await PowerCmd.Remove(this);
            return;
        }

        await FriendlyAmalgamCmd.ExecuteSingleTargetAttack(
            new ThrowingPlayerChoiceContext(),
            base.Owner,
            randomEnemy,
            Amount,
            "Attack",
            0.6f,
            "vfx/vfx_attack_blunt");

        await PowerCmd.Remove(this);
    }
}

