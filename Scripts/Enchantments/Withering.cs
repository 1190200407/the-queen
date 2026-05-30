using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace ComicChess.TheQueen;

/// <summary>凋萎：附魔牌获得 <see cref="CardKeyword.Retain"/>；每被保留一次 <see cref="Amount"/> 减 1，为 0 时消耗。</summary>
public sealed class Withering : QueenEnchantmentModel
{
	public override bool ShowAmount => true;

	public override bool HasExtraCardText => true;

	public override bool IsStackable => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Retain)];
	protected override void OnEnchant()
	{
		if (Card is null)
		{
			return;
		}

		if (!Card.Keywords.Contains(CardKeyword.Retain))
		{
			CardCmd.ApplyKeyword(Card, CardKeyword.Retain);
		}
	}

	public override async Task AfterFlush(
		PlayerChoiceContext choiceContext,
		Player player,
		IReadOnlyCollection<CardModel> flushedCards,
		IReadOnlyCollection<CardModel> retainedCards)
	{
		_ = flushedCards;
		if (Card is null || Card.Owner != player || !retainedCards.Contains(Card))
		{
			return;
		}

		if (Amount <= 0)
		{
			return;
		}

		Amount--;
		SyncDeckVersionAmount();
		RefreshEnchantmentDisplay();

		if (Amount > 0)
		{
			return;
		}

		await CardCmd.Exhaust(choiceContext, Card);
	}

	private void SyncDeckVersionAmount()
	{
		if (Card?.DeckVersion?.Enchantment is Withering deckWithering)
		{
			deckWithering.Amount = Amount;
		}
	}

	private void RefreshEnchantmentDisplay()
	{
		if (Card?.Pile is not { } pile)
		{
			return;
		}

		NCard.FindOnTable(Card)?.UpdateVisuals(pile.Type, CardPreviewMode.Normal);
	}
}
