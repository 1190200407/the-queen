using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace ComicChess.TheQueen;

/// <summary>聚合体「进攻意图」如何选承伤者；决定预览共识池与 <see cref="AmalgamOffenseIntentAction"/> 结算。</summary>
public enum AmalgamOffenseTargetingMode
{
    /// <summary>从仍存活的敌人中随机一名（默认）。</summary>
    RandomAliveEnemy,

    /// <summary>对每名仍存活的敌人各打一次（万灵破军）。</summary>
    AllAliveEnemies,

    /// <summary>仅打击带 <see cref="AmalgamPickLockPower"/> 的敌人；无标记时回退为随机。</summary>
    LockedMarkedEnemy
}

public static class AmalgamOffenseTargeting
{
    /// <summary>万灵破军优先于锁定：有能力时始终按全体结算与预览。</summary>
    public static AmalgamOffenseTargetingMode ResolveMode(CombatState combatState, Player queen)
    {
        if (queen.Creature.GetPower<AmalgamArmyBreakPower>() != null)
        {
            return AmalgamOffenseTargetingMode.AllAliveEnemies;
        }

        if (FindMarkedEnemy(combatState) is { IsAlive: true })
        {
            return AmalgamOffenseTargetingMode.LockedMarkedEnemy;
        }

        return AmalgamOffenseTargetingMode.RandomAliveEnemy;
    }

    public static Creature? FindMarkedEnemy(CombatState combatState) =>
        combatState.Enemies.FirstOrDefault(e => e.IsAlive && e.GetPower<AmalgamPickLockPower>() != null);

    /// <summary>与 <see cref="AmalgamIntentDamagePreview.PreviewOutgoingConsensusAmongReceivers"/> 的候选承伤者一致。</summary>
    public static Creature[] GetPreviewReceiverPool(CombatState combatState, Player queen)
    {
        Creature[] alive = combatState.Enemies.Where(e => e.IsAlive).ToArray();
        return ResolveMode(combatState, queen) switch
        {
            AmalgamOffenseTargetingMode.AllAliveEnemies => alive,
            AmalgamOffenseTargetingMode.LockedMarkedEnemy => FindMarkedEnemy(combatState) is { IsAlive: true } m ? [m] : [],
            _ => alive,
        };
    }
}
