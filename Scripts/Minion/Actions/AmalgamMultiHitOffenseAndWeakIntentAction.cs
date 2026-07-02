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

/// <summary>????????? + ??????? 1 ??????</summary>
public sealed class AmalgamMultiHitOffenseAndWeakIntentAction : AmalgamActionModel
{
    public override string Key => "offense_multi_and_weak";

    private int _hitCount;
    private decimal _weak;
    private Creature? _forcedTarget;

    public AmalgamMultiHitOffenseAndWeakIntentAction()
    {
    }

    public AmalgamMultiHitOffenseAndWeakIntentAction(decimal damagePerHit, int hitCount, decimal weakStacks)
        : this()
    {
        Amount = damagePerHit;
        _hitCount = hitCount;
        _weak = weakStacks;
    }

    public AmalgamMultiHitOffenseAndWeakIntentAction(decimal damagePerHit, int hitCount, decimal weakStacks, Creature? forcedTarget)
        : this(damagePerHit, hitCount, weakStacks)
    {
        _forcedTarget = forcedTarget;
    }

    protected override void ResetForInit()
    {
        base.ResetForInit();
        _hitCount = 0;
        _weak = 0m;
        _forcedTarget = null;
    }

    public override bool Init(decimal amount) => false;

    public override bool Init(object[] args)
    {
        if (!AmalgamActionArgs.TryGetDecimal(args, 0, out decimal damagePerHit)
            || !AmalgamActionArgs.TryGetInt(args, 1, out int hitCount)
            || !AmalgamActionArgs.TryGetDecimal(args, 2, out decimal weakStacks)
            || !AmalgamActionArgs.IsPositive(damagePerHit)
            || hitCount <= 0
            || !AmalgamActionArgs.IsPositive(weakStacks))
        {
            return false;
        }

        ResetForInit();
        Amount = damagePerHit;
        _hitCount = hitCount;
        _weak = weakStacks;
        _forcedTarget = AmalgamActionArgs.TryGetCreature(args, 3);
        return true;
    }

    protected override MoveState CreateMoveState()
    {
        // action ???? intent ?? 2 ???? + ????? UI?
        return new MoveState(
            "AMALGAM_INTENT_OFFENSE_MULTI_AND_WEAK",
            _ => Task.CompletedTask,
            new AmalgamMultiHitAttackIntent(Amount, _hitCount),
            new AmalgamApplyWeakIntent(_weak));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        CombatState? combatState = amalgam.CombatState;
        if (combatState == null || amalgam.PetOwner is not Player queen || !queen.Creature.IsAlive)
        {
            return;
        }

        Creature[] alive = combatState.Enemies.Where(e => e.IsAlive).ToArray();
        if (alive.Length == 0 || Amount <= 0m || _hitCount <= 0 || _weak <= 0m)
        {
            return;
        }

        // ????????????????????? + ???
        await FriendlyAmalgamCmd.ExecuteMultiHitOffense(
            choiceContext,
            amalgam,
            _forcedTarget,
            Amount,
            _hitCount,
            "PowerAttack",
            0.7f,
            "vfx/vfx_attack_blunt",
            "event:/sfx/enemy/enemy_attacks/torch_head_amalgam/torch_head_amalgam_beam");

        // ?????????????? ApplyWeakIntentAction ?????
        AmalgamOffenseTargetingMode mode = AmalgamOffenseTargeting.ResolveMode(combatState, queen);
        Creature applier = queen.Creature;

        if (_forcedTarget is { IsAlive: true } forcedTarget && alive.Contains(forcedTarget))
        {
            await PowerCmd.Apply<WeakPower>(forcedTarget, _weak, applier, null);
            return;
        }

        if (mode == AmalgamOffenseTargetingMode.AllAliveEnemies)
        {
            foreach (Creature enemy in alive)
            {
                await PowerCmd.Apply<WeakPower>(enemy, _weak, applier, null);
            }

            return;
        }

        if (mode == AmalgamOffenseTargetingMode.LockedMarkedEnemy)
        {
            Creature? marked = AmalgamOffenseTargeting.FindMarkedEnemy(combatState);
            if (marked is { IsAlive: true })
            {
                await PowerCmd.Apply<WeakPower>(marked, _weak, applier, null);
            }

            return;
        }

        Creature? randomEnemy = FriendlyAmalgamCmd.NextRandomHittableEnemy(queen, alive);
        if (randomEnemy is not { IsAlive: true })
        {
            return;
        }

        await PowerCmd.Apply<WeakPower>(randomEnemy, _weak, applier, null);
    }
}
