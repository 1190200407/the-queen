using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

internal static class DiscardBeforeHandGenerateHelper
{
	public static async Task MaybeDiscardForHandGenerate(Player owner, PlayerChoiceContext? choiceContext, CardModel? selectionSource)
	{
		ArgumentNullException.ThrowIfNull(owner);
		if (owner.Character is not QueenCharacter)
		{
			return;
		}

		if (owner.Creature.GetPower<DiscardBeforeHandGeneratePower>() == null)
		{
			return;
		}

		CardPile hand = PileType.Hand.GetPile(owner);
		if (hand.Cards.Count == 0)
		{
			return;
		}

		PlayerChoiceContext ctx = choiceContext ?? new BlockingPlayerChoiceContext();
		AbstractModel source = (AbstractModel?)selectionSource ?? ModelDb.Power<DiscardBeforeHandGeneratePower>();
		IEnumerable<CardModel> toDiscard = await CardSelectCmd.FromHandForDiscard(
			ctx,
			owner,
			new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1),
			null,
			source);

		await CardCmd.Discard(ctx, toDiscard);
	}
}
