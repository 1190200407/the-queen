using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;

namespace ComicChess.TheQueen;

/// <summary>
/// 本场战斗中友方聚合体若已通过逃跑离场，则禁止对该玩家再次执行 <see cref="FriendlyAmalgamCmd.Summon"/>。
/// 由打出 <see cref="Flee"/>、<see cref="AmalgamEscapePower"/> 触发（经 <see cref="Flee.RemoveFromCombatWithoutEscapeFlag"/> 与/或能力内显式 <see cref="MarkAmalgamFled"/>）。
/// 数据挂在 <see cref="CombatState"/> 上随战斗结束 GC，无需手动清空。
/// </summary>
public static class AmalgamFledSummonBlock
{
    private sealed class Row
    {
        public readonly Dictionary<Player, bool> ByOwner = new();
    }

    private static readonly ConditionalWeakTable<CombatState, Row> s_rows = new();

    public static void MarkAmalgamFled(CombatState combatState, Player amalgamOwner)
    {
        Row row = s_rows.GetValue(combatState, _ => new Row());
        row.ByOwner[amalgamOwner] = true;
    }

    public static bool IsSummonBlocked(CombatState combatState, Player owner) =>
        s_rows.TryGetValue(combatState, out Row? row)
        && row.ByOwner.TryGetValue(owner, out bool fled)
        && fled;
}
