using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(QueenCardPool))]
public sealed class HundredHandsBanquet : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override bool HasEnergyCostX => true;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromCard<Scratch>(upgrade: base.IsUpgraded),
		ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade),
		HoverTipFactory.FromPower<SoulLampPower>(),
		.. HoverTipFactory.FromAffliction<Bound>(),
	];

	public HundredHandsBanquet()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.CombatState == null || base.Owner == null)
		{
			return;
		}

		int xValue = ResolveEnergyXValue();
		if (xValue > 0)
		{
			await CreateFadeScratchesInHand(base.Owner, base.CombatState, xValue, base.IsUpgraded);
			await QueenCardCmd.AddSoulLamp(choiceContext, base.Owner, xValue);
		}
	}

	private static async Task CreateFadeScratchesInHand(
		Player owner,
		ICombatState combatState,
		int count,
		bool isUpgraded)
	{
		CardKeyword fadeKeyword = ModKeywordRegistry.GetCardKeyword(QueenKeyword.Fade);
		for (int i = 0; i < count; i++)
		{
			CardModel scratch = combatState.CreateCard<Scratch>(owner);
			scratch.AddModKeyword(fadeKeyword);
			if (isUpgraded)
			{
				CardCmd.Upgrade(scratch);
			}

			await CardPileCmd.AddGeneratedCardToCombat(scratch, PileType.Hand, owner);
		}
	}

	internal static async Task CreateInHandInternal(Player owner, CombatState? combatState, bool isUpgraded = false)
	{
		if (combatState == null)
		{
			return;
		}

		await QueenCardCmd.CreateInHand<HandOfSeizure>(owner, combatState, isUpgraded);
		await QueenCardCmd.CreateInHand<HandOfRefusal>(owner, combatState, isUpgraded);
	}
}
