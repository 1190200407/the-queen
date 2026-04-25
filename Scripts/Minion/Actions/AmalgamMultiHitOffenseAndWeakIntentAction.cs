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

/// <summary>一个意图：多段进攻 + 施加虚弱（只占 1 个意图槽）。</summary>
public sealed class AmalgamMultiHitOffenseAndWeakIntentAction : AmalgamActionModel
{
    private const string RepeatParam = "repeat";
    private readonly int _hitCount;
    private readonly decimal _weak;
    private readonly Creature? _forcedTarget;

    public AmalgamMultiHitOffenseAndWeakIntentAction(decimal damagePerHit, int hitCount, decimal weakStacks)
        : base(new System.Collections.Generic.Dictionary<string, decimal>
        {
            [AmountParam] = damagePerHit,
            [RepeatParam] = hitCount
        })
    {
        _hitCount = (int)GetParameterOrDefault(RepeatParam, 0m);
        _weak = weakStacks;
    }

    public AmalgamMultiHitOffenseAndWeakIntentAction(decimal damagePerHit, int hitCount, decimal weakStacks, Creature? forcedTarget)
        : this(damagePerHit, hitCount, weakStacks)
    {
        _forcedTarget = forcedTarget;
    }

    public override LocString IntentTitle => new("intents", "AMALGAM_MULTI_ATTACK.title");

    public override LocString GetIntentDescription()
    {
        LocString desc = new("intents", "AMALGAM_MULTI_ATTACK.fallback_description");
        desc.Add("Amount", Amount);
        desc.Add("Repeat", _hitCount);
        return desc;
    }

    protected override MoveState CreateMoveState()
    {
        // action 合并，但 intent 保留 2 个（进攻 + 虚弱）用于 UI。
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

        // 先按现有逻辑打多段伤害（内部会处理目标选择 + 动画）
        await FriendlyAmalgamCmd.ExecuteMultiHitOffense(choiceContext, amalgam, Amount, _hitCount);

        // 再对同一目标规则施加虚弱（与 ApplyWeakIntentAction 保持一致）
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

        Creature randomEnemy = alive[System.Random.Shared.Next(alive.Length)];
        await PowerCmd.Apply<WeakPower>(randomEnemy, _weak, applier, null);
    }
}

