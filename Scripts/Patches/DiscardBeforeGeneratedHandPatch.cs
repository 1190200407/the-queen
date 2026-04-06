using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 拦截 <see cref="CardPileCmd.AddGeneratedCardsToCombat"/>（单卡 <see cref="CardPileCmd.AddGeneratedCardToCombat"/> 亦走此路径），
/// 在女王持有 <see cref="DiscardBeforeHandGeneratePower"/> 且本次仅生成一张入手牌时先弃 1 张。
/// </summary>
/// <remarks>
/// 勿直接补丁 <c>AddGeneratedCardToCombat</c>：其返回 <c>Task&lt;CardPileAddResult&gt;</c> 且 <c>CardPileAddResult</c> 为 struct，
/// Harmony 对 async 方法替换该返回值时易触发 IL 编译异常。
/// </remarks>
[HarmonyPatch]
internal static class DiscardBeforeGeneratedHandPatch
{
	private static Func<IEnumerable<CardModel>, PileType, bool, CardPilePosition, Task<IReadOnlyList<CardPileAddResult>>>? _originalAddGeneratedMany;

	internal static void CacheOriginalDelegateIfAvailable()
	{
		MethodInfo? mi = AccessTools.DeclaredMethod(
			typeof(CardPileCmd),
			nameof(CardPileCmd.AddGeneratedCardsToCombat),
			[typeof(IEnumerable<CardModel>), typeof(PileType), typeof(bool), typeof(CardPilePosition)]);
		if (mi == null)
		{
			return;
		}

		_originalAddGeneratedMany = AccessTools.MethodDelegate<Func<IEnumerable<CardModel>, PileType, bool, CardPilePosition, Task<IReadOnlyList<CardPileAddResult>>>>(mi);
	}

	private static bool Prepare() =>
		AccessTools.DeclaredMethod(
			typeof(CardPileCmd),
			nameof(CardPileCmd.AddGeneratedCardsToCombat),
			[typeof(IEnumerable<CardModel>), typeof(PileType), typeof(bool), typeof(CardPilePosition)]) != null;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.AddGeneratedCardsToCombat), typeof(IEnumerable<CardModel>), typeof(PileType), typeof(bool), typeof(CardPilePosition))]
	private static bool Prefix(
		IEnumerable<CardModel> cards,
		PileType newPileType,
		bool addedByPlayer,
		CardPilePosition position,
		ref Task<IReadOnlyList<CardPileAddResult>> __result)
	{
		if (_originalAddGeneratedMany == null || newPileType != PileType.Hand || !addedByPlayer)
		{
			return true;
		}

		List<CardModel> list = cards as List<CardModel> ?? cards.ToList();
		if (list.Count != 1)
		{
			return true;
		}

		CardModel card = list[0];
		Player? owner = card.Owner;
		if (owner?.Character is not QueenCharacter || owner.Creature.GetPower<DiscardBeforeHandGeneratePower>() == null)
		{
			return true;
		}

		__result = RunWithDiscardFirst(_originalAddGeneratedMany, list, newPileType, addedByPlayer, position);
		return false;
	}

	private static async Task<IReadOnlyList<CardPileAddResult>> RunWithDiscardFirst(
		Func<IEnumerable<CardModel>, PileType, bool, CardPilePosition, Task<IReadOnlyList<CardPileAddResult>>> original,
		List<CardModel> cards,
		PileType newPileType,
		bool addedByPlayer,
		CardPilePosition position)
	{
		Player owner = cards[0].Owner!;
		await DiscardBeforeHandGenerateHelper.MaybeDiscardForHandGenerate(owner, choiceContext: null, selectionSource: null);
		return await original(cards, newPileType, addedByPlayer, position);
	}
}
