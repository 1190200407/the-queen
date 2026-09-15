using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace ComicChess.TheQueen;

[RegisterSingleton]
public sealed class SoulLampSingleton : HookedSingletonModel, ISecondaryResourceHookListener
{
	public SoulLampSingleton()
		: base(HookType.Combat)
	{
	}

	public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (!ShouldZeroBoundCardCost(card))
		{
			return false;
		}

		modifiedCost = 0m;
		return true;
	}

	public override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (!ShouldZeroBoundCardCost(card))
		{
			return false;
		}

		modifiedCost = 0m;
		return true;
	}

	public override async Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (!ShouldSpendSoulLamp(cardPlay, out CardModel card))
		{
			return;
		}

		await SecondaryResourceCmd.Spend(
			card.Owner!,
			SoulLampResources.SoulLampId,
			1,
			card,
			card);
	}

	public async Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
	{
		if (!IsSoulLamp(context.Definition))
		{
			return;
		}

		NQueenEnergyCounter.TryRefresh(context.Player);
		await SoulLampHook.AfterAmountChanged(
			context.CombatState,
			new ThrowingPlayerChoiceContext(),
			context.Player,
			context.Delta,
			context.Player.Creature,
			context.Source as CardModel);
	}

	private static bool ShouldZeroBoundCardCost(CardModel? card)
	{
		if (card == null || card.EnergyCost.CostsX)
		{
			return false;
		}

		if (card.Owner == null || card.Affliction is not Bound)
		{
			return false;
		}

		if (!SoulLampResources.HasAny(card.Owner))
		{
			return false;
		}

		return card.Pile?.Type is PileType.Hand or PileType.Play;
	}

	private static bool ShouldSpendSoulLamp(CardPlay cardPlay, out CardModel card)
	{
		card = cardPlay.Card;
		if (cardPlay.IsAutoPlay || !cardPlay.IsFirstInSeries)
		{
			return false;
		}

		if (card.Owner == null || card.Affliction is not Bound)
		{
			return false;
		}

		if (card.Pile?.Type is not (PileType.Hand or PileType.Play))
		{
			return false;
		}

		return SoulLampResources.HasAny(card.Owner);
	}

	private static bool IsSoulLamp(SecondaryResourceDefinition definition) =>
		string.Equals(definition.Id, SoulLampResources.SoulLampId, System.StringComparison.OrdinalIgnoreCase);
}
