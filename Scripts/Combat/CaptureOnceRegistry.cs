using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace ComicChess.TheQueen;

/// <summary>战斗内「每名玩家对每个怪物实例（<see cref="Creature"/>）只能成功捕获一次」的去重表。</summary>
internal static class CaptureOnceRegistry
{
    private readonly record struct PlayerCreatureKey(Player Player, Creature Victim);

    private sealed class KeyComparer : IEqualityComparer<PlayerCreatureKey>
    {
        public static readonly KeyComparer Instance = new();

        public bool Equals(PlayerCreatureKey x, PlayerCreatureKey y) =>
            ReferenceEquals(x.Player, y.Player) && ReferenceEquals(x.Victim, y.Victim);

        public int GetHashCode(PlayerCreatureKey obj) =>
            HashCode.Combine(RuntimeHelpers.GetHashCode(obj.Player), RuntimeHelpers.GetHashCode(obj.Victim));
    }

<<<<<<< HEAD
    private static readonly ConditionalWeakTable<CombatState, HashSet<PlayerCreatureKey>> CapturedByCombat = new();

    public static bool HasCaptured(Player player, Creature victim, CombatState cs) =>
=======
    private static readonly ConditionalWeakTable<ICombatState, HashSet<PlayerCreatureKey>> CapturedByCombat = new();

    public static bool HasCaptured(Player player, Creature victim, ICombatState cs) =>
>>>>>>> beta
        CapturedByCombat.TryGetValue(cs, out HashSet<PlayerCreatureKey>? set)
        && set.Contains(new PlayerCreatureKey(player, victim));

    /// <summary>
    /// 若该 <paramref name="player"/> 尚未捕获过此 <paramref name="victim"/> 实例，则标记并返回 true。
    /// </summary>
<<<<<<< HEAD
    public static bool TryMarkCaptured(Player player, Creature victim, CombatState cs)
=======
    public static bool TryMarkCaptured(Player player, Creature victim, ICombatState cs)
>>>>>>> beta
    {
        HashSet<PlayerCreatureKey> set = CapturedByCombat.GetValue(cs, static _ => new HashSet<PlayerCreatureKey>(KeyComparer.Instance));
        return set.Add(new PlayerCreatureKey(player, victim));
    }
}
