using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>虹吸：伤害；目标每有 2 个负面能力（Debuff），回复 1 点能量。</summary>
[Pool(typeof(QueenCardPool))]
public sealed class Siphon : QueenCardModel
{
	private const int energyCost = 2;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;
	private const int debuffKindsPerEnergy = 2;

	protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(14m, ValueProp.Move),
		new EnergyVar(1),
		new CalculationBaseVar(0m),
		new CalculationExtraVar(1m),
		new CalculatedVar("CalculatedEnergy").WithMultiplier((CardModel card, Creature? creature) =>
		{
			_ = card;
			int debuffKinds = creature?.Powers.Count(static p => p.Type == PowerType.Debuff) ?? 0;
			return debuffKinds / debuffKindsPerEnergy;
		})
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [base.EnergyHoverTip];

	public Siphon()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		Creature target = cardPlay.Target;
		if (!target.IsAlive)
		{
			return;
		}

		int debuffKinds = target.Powers.Count(static p => p.Type == PowerType.Debuff);
		int energyGain = debuffKinds / debuffKindsPerEnergy;
		if (energyGain > 0)
		{
			await PlayerCmd.GainEnergy(energyGain, base.Owner);
		}
		
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(4m);
	}
}
