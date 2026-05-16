using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

/// <summary>
/// 替换原版 <see cref="CardCmd.DiscardAndDraw"/>：对弃牌列表做快照与去重，按每张牌的 Owner 取弃牌堆，
/// 跳过已脱离战斗或已无 Owner 的项，避免与「消逝」改道消耗、或其它 Hook 中途改牌堆时迭代不稳定。
/// </summary>
[HarmonyPatch(typeof(CardCmd), nameof(CardCmd.DiscardAndDraw))]
internal static class CardCmdDiscardAndDrawPatch
{
	[HarmonyPrefix]
	private static bool Prefix(
		PlayerChoiceContext choiceContext,
		IEnumerable<CardModel> cardsToDiscard,
		int cardsToDraw,
		ref Task __result)
	{
		ArgumentNullException.ThrowIfNull(cardsToDiscard);
		__result = DiscardAndDrawImpl(choiceContext, cardsToDiscard, cardsToDraw);
		return false;
	}

	private static async Task DiscardAndDrawImpl(
		PlayerChoiceContext choiceContext,
		IEnumerable<CardModel> cardsToDiscard,
		int cardsToDraw)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}

		// 立即物化快照，避免调用方传入的 IEnumerable 在 await 后再次枚举时变化
		List<CardModel> discardCards = cardsToDiscard.Where(c => c != null).Distinct().ToList();
		if (discardCards.Count == 0)
		{
			return;
		}

		CardModel? anchor = discardCards.FirstOrDefault(c => c.Owner != null);
		if (anchor?.Owner == null)
		{
			return;
		}

		Player primaryOwner = anchor.Owner;
		CombatState? combatState = anchor.CombatState ?? primaryOwner.Creature?.CombatState;
		if (combatState == null)
		{
			return;
		}

		List<CardModel> slyCards = new List<CardModel>();
		List<CardModel> fadeCards = new List<CardModel>();

		foreach (CardModel card in discardCards)
		{
			Player? owner = card.Owner;
			if (owner == null || owner.Creature == null)
			{
				continue;
			}

			CombatState? resolvedCombat = card.CombatState ?? owner.Creature.CombatState;
			if (resolvedCombat == null || !resolvedCombat.ContainsCard(card))
			{
				continue;
			}

			if (card.HasBeenRemovedFromState)
			{
				continue;
			}

			CardPile discardPile = PileType.Discard.GetPile(owner);

			if (card.IsSlyThisTurn)
			{
				slyCards.Add(card);
			}

			// 消逝卡牌直接进入消耗堆，不在这做处理
			if (card.HasModKeyword(QueenKeyword.Fade))
			{
				fadeCards.Add(card);
			}
			else
			{
				await CardPileCmd.Add(card, discardPile);
				CombatManager.Instance.History.CardDiscarded(combatState, card);
				await Hook.AfterCardDiscarded(combatState, choiceContext, card);
			}
		}

		PileType.Discard.GetPile(primaryOwner).InvokeContentsChanged();

		if (cardsToDraw > 0)
		{
			await CardPileCmd.Draw(choiceContext, cardsToDraw, primaryOwner);
		}

		foreach (CardModel item in slyCards)
		{
			// 不走 CardCmd.AutoPlay：BaseLib 对其打的 AnyPlayer 补丁会引用已移除的 CombatState，JIT 抛 TypeLoadException（联机弃灵巧牌等）。
			await CardAutoPlayDirect.AutoPlayAsync(choiceContext, item, target: null, AutoPlayType.SlyDiscard);
		}

		foreach (CardModel item in fadeCards)
		{
			await CardPileCmd.Add(item, PileType.Exhaust.GetPile(primaryOwner));
			CombatManager.Instance.History.CardExhausted(combatState, item);
			await Hook.AfterCardExhausted(combatState, choiceContext, item, causedByEthereal: false);
		}
	}

}
