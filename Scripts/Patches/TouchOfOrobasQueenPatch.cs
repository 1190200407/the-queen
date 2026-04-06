using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace ComicChess.TheQueen;

/// <summary>
/// 将女王初始遗物纳入 <see cref="TouchOfOrobas"/> 的升级映射（原版字典不含 mod 遗物）。
/// </summary>
[HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.GetUpgradedStarterRelic))]
internal static class TouchOfOrobasQueenPatch
{
	[HarmonyPostfix]
	private static void Postfix(RelicModel starterRelic, ref RelicModel __result)
	{
		if (starterRelic.Id != ModelDb.Relic<FirstGiftRelic>().Id)
		{
			return;
		}

		__result = ModelDb.Relic<QueensGraceRelic>().ToMutable();
	}
}
