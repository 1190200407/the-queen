using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>下一张带 <see cref="QueenCardTags.LearnIntent"/> 标签的牌耗能视为 0；打出后消耗一层。</summary>
public sealed class EvolutionsLawFreeLearnIntentPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (base.Amount <= 0)
		{
			return false;
		}

		if (card.Owner?.Creature != base.Owner)
		{
			return false;
		}

		if (!card.Tags.Contains(QueenCardTags.LearnIntent))
		{
			return false;
		}

		if (card.EnergyCost.CostsX)
		{
			return false;
		}

		switch (card.Pile?.Type)
		{
			case PileType.Hand:
			case PileType.Play:
				break;
			default:
				return false;
		}

		modifiedCost = default;
		return true;
	}

	public override async Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (!cardPlay.IsFirstInSeries)
		{
			return;
		}

		CardModel card = cardPlay.Card;
		if (card.Owner?.Creature != base.Owner)
		{
			return;
		}

		if (!card.Tags.Contains(QueenCardTags.LearnIntent))
		{
			return;
		}

		switch (card.Pile?.Type)
		{
			case PileType.Hand:
			case PileType.Play:
				break;
			default:
				return;
		}

		await PowerCmd.Decrement(this);
	}
}
