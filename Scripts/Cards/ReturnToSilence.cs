using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(QueenCardPool))]
public sealed class ReturnToSilence : QueenCardModel
{
	private const int energyCost = 2;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new CalculationBaseVar(0m),
		new ExtraDamageVar(1m),
		new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (CardModel _, Creature? target) => GetTriadStackSum(target)),
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<PoisonPower>(),
		HoverTipFactory.FromPower<DoomPower>(),
		HoverTipFactory.FromPower<DemisePower>(),
	];

	public ReturnToSilence()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

		await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}

	private static decimal GetTriadStackSum(Creature? target)
	{
		if (target is null)
		{
			return 0m;
		}

		return target.GetPowerAmount<PoisonPower>()
			+ target.GetPowerAmount<DoomPower>()
			+ target.GetPowerAmount<DemisePower>();
	}
}
