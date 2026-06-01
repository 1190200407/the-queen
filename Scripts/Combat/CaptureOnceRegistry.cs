using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;

namespace ComicChess.TheQueen;

/// <summary>战斗内「每名玩家对同一种怪物 Id 只能成功捕获一次」的去重表。</summary>
internal static class CaptureOnceRegistry
{
    private readonly record struct PlayerMonsterKey(Player Player, string MonsterId);

    private sealed class KeyComparer : IEqualityComparer<PlayerMonsterKey>
    {
        public static readonly KeyComparer Instance = new();

        public bool Equals(PlayerMonsterKey x, PlayerMonsterKey y) =>
            ReferenceEquals(x.Player, y.Player)
            && string.Equals(x.MonsterId, y.MonsterId, StringComparison.Ordinal);

        public int GetHashCode(PlayerMonsterKey obj) =>
            HashCode.Combine(RuntimeHelpers.GetHashCode(obj.Player), obj.MonsterId);
    }

    private static readonly ConditionalWeakTable<CombatState, HashSet<PlayerMonsterKey>> CapturedByCombat = new();

    public static bool HasPlayerCapturedMonster(Player player, string monsterId, CombatState cs)
    {
        return CapturedByCombat.TryGetValue(cs, out HashSet<PlayerMonsterKey>? set)
            && set.Contains(new PlayerMonsterKey(player, monsterId));
    }

    /// <summary>
    /// 若该 <paramref name="player"/> 在本场战斗中尚未捕获过此 <paramref name="monsterId"/>，则标记并返回 true。
    /// </summary>
    public static bool TryMarkPlayerCapturedMonster(Player player, string monsterId, CombatState cs)
    {
        HashSet<PlayerMonsterKey> set = CapturedByCombat.GetValue(cs, static _ => new HashSet<PlayerMonsterKey>(KeyComparer.Instance));
        return set.Add(new PlayerMonsterKey(player, monsterId));
    }
}
