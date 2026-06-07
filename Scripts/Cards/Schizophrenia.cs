using System;
using System.Collections.Generic;
using System.Linq;
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

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>精神分裂：施加易伤与虚弱，再将目标该 debuff 的总层数固定拆成「原版 + 类」两份并均分；消耗。</summary>
[RegisterCard(typeof(QueenCardPool))]
public sealed class Schizophrenia : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new PowerVar<VulnerablePower>(2m),
		new PowerVar<WeakPower>(2m),
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<VulnerablePower>(),
		HoverTipFactory.FromPower<WeakPower>(),
	];

	public Schizophrenia()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

		Creature target = cardPlay.Target;
		Creature applier = base.Owner.Creature;

		decimal vulnerableStacks = base.DynamicVars.Vulnerable.BaseValue;
		decimal weakStacks = base.DynamicVars.Weak.BaseValue;

		await PowerCmd.Apply<VulnerablePower>(target, vulnerableStacks, applier, this);
		await PowerCmd.Apply<WeakPower>(target, weakStacks, applier, this);
		await RedistributeIntoTwoParts<VulnerablePower, SplitVulnerablePower>(choiceContext, target, applier, this);
		await RedistributeIntoTwoParts<WeakPower, SplitWeakPower>(choiceContext, target, applier, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Vulnerable.UpgradeValueBy(2m);
		base.DynamicVars.Weak.UpgradeValueBy(2m);
	}

	/// <summary>汇总原版与类 debuff 总层数，清空后固定拆成两份并均分（原版向上取整，类 debuff 向下取整）。</summary>
	private static async Task RedistributeIntoTwoParts<TOriginal, TSplit>(
		PlayerChoiceContext choiceContext,
		Creature target,
		Creature applier,
		CardModel? cardSource)
		where TOriginal : PowerModel
		where TSplit : PowerModel
	{
		int total = target.GetPowerAmount<TOriginal>();
		foreach (TSplit split in target.GetPowerInstances<TSplit>())
		{
			total += split.Amount;
		}

		if (total <= 0)
		{
			return;
		}

		foreach (TOriginal original in target.GetPowerInstances<TOriginal>().ToList())
		{
			await PowerCmd.Remove(original);
		}

		foreach (TSplit split in target.GetPowerInstances<TSplit>().ToList())
		{
			await PowerCmd.Remove(split);
		}

		int originalStacks = (total + 1) / 2;
		int splitStacks = total / 2;

		if (originalStacks > 0)
		{
			await PowerCmd.Apply<TOriginal>(target, originalStacks, applier, cardSource);
		}

		if (splitStacks > 0)
		{
			await PowerCmd.Apply<TSplit>(target, splitStacks, applier, cardSource);
		}
	}
}
