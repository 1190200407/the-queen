using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 随机变形候选：排除「自身不能作为牌组常驻」的卡模板。
/// 以 <see cref="AbstractModel.ShouldAddToDeck"/> 对「自身」判断（如 <c>card is not T</c> 的放生、寄生、纸伤难愈）；遗物战斗外变形不会走 <see cref="CardModel.CanBeGeneratedInCombat"/>。
/// </summary>
[HarmonyPatch(typeof(CardFactory), "GetFilteredTransformationOptions", new Type[]
{
	typeof(CardModel),
	typeof(IEnumerable<CardModel>),
	typeof(bool)
})]
internal static class ExcludeNonDeckableFromCardTransformPatch
{
	[HarmonyPostfix]
	private static void Postfix(ref CardModel[] __result)
	{
		if (__result is not { Length: > 0 })
		{
			return;
		}

		CardModel[] without = __result.Where(static c => c.ShouldAddToDeck(c)).ToArray();
		if (without.Length > 0)
		{
			__result = without;
		}
	}
}
