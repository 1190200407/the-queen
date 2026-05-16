using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>星盘：获得能量并生成多张 <see cref="MorningStar"/>；消耗�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class Astrolabe : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new EnergyVar(2),
		new CardsVar(2),
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<SoulLampPower>(),
		.. HoverTipFactory.FromAffliction<Bound>(),
		HoverTipFactory.FromCard<MorningStar>(),
		HoverTipFactory.FromCard<EveningStar>()
	];

	public Astrolabe()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = choiceContext;
		_ = cardPlay;
		if (base.CombatState is null)
		{
			return;
		}

		await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
		int n = base.DynamicVars.Cards.IntValue;
		for (int i = 0; i < n; i++)
		{
			await QueenCardCmd.CreateInHand<MorningStar>(base.Owner, base.CombatState, isUpgraded: false);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Energy.UpgradeValueBy(1m);
		base.DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
