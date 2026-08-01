using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class CardCmdDiscardAndDrawPatch : IPatchMethod
{
	private static readonly CardKeyword FadeKeyword = ModKeywordRegistry.GetCardKeyword(QueenKeyword.Fade);

	public static string PatchId => "thequeen_card_cmd_discard_and_draw";
	public static string Description => "Replace DiscardAndDraw with snapshot-safe fade/sly flow";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CardCmd), nameof(CardCmd.DiscardAndDraw)),
	];

	public static bool Prefix(
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
		ICombatState? combatState = anchor.CombatState ?? primaryOwner.Creature?.CombatState;
		if (combatState == null)
		{
			return;
		}

		List<CardModel> slyCards = new();
		List<CardModel> fadeCards = new();

		foreach (CardModel card in discardCards)
		{
			Player? owner = card.Owner;
			if (owner == null || owner.Creature == null)
			{
				continue;
			}

			ICombatState? resolvedCombat = card.CombatState ?? owner.Creature.CombatState;
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

			if (card.HasModKeyword(FadeKeyword))
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
