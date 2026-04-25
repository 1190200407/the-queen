using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace ComicChess.TheQueen;

/// <summary>战斗内「每个怪只能被捕获一次」的去重表（按 Creature 引用）。</summary>
internal static class CaptureOnceRegistry
{
    private sealed class RefComparer : IEqualityComparer<Creature>
    {
        public static readonly RefComparer Instance = new();
        public bool Equals(Creature? x, Creature? y) => ReferenceEquals(x, y);
        public int GetHashCode(Creature obj) => RuntimeHelpers.GetHashCode(obj);
    }

    private static readonly ConditionalWeakTable<CombatState, HashSet<Creature>> CapturedByCombat = new();

    /// <summary>
    /// 若该 <paramref name="victim"/> 在本场战斗中尚未捕获，则标记为已捕获并返回 true；否则返回 false。
    /// </summary>
    public static bool TryMarkCaptured(Creature victim, CombatState cs)
    {
        HashSet<Creature> set = CapturedByCombat.GetValue(cs, static _ => new HashSet<Creature>(RefComparer.Instance));
        return set.Add(victim);
    }
}

