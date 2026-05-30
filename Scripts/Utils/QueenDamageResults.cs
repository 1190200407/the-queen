using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace ComicChess.TheQueen;

/// <summary>斩杀判定：<see cref="AttackCommand.Results"/> 现为按段/批次分组的嵌套伤害结果。</summary>
internal static class QueenDamageResults
{
	public static bool AnyTargetKilled(AttackCommand attackCommand) =>
		AnyTargetKilled(attackCommand.Results);

	public static bool AnyTargetKilled(IEnumerable<IEnumerable<DamageResult>>? nestedResults) =>
		nestedResults?.Any(static batch => batch.Any(static r => r.WasTargetKilled)) ?? false;

	public static bool AnyTargetKilled(IEnumerable<DamageResult>? results) =>
		results?.Any(static r => r.WasTargetKilled) ?? false;

	public static int CountTargetKilled(AttackCommand attackCommand) =>
		attackCommand.Results.Sum(static batch => batch.Count(static r => r.WasTargetKilled));
}
