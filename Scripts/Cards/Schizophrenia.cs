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

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>精神分裂：施加易伤与虚弱，再分别将易伤/类易伤、虚弱/类虚弱各层数平分（仅 1 层时不分割）；消耗。</summary>
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
		HoverTipFactory.FromPower<WeakPower>()
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

		await PowerCmd.Apply<VulnerablePower>(choiceContext, target, vulnerableStacks, applier, this);
		await PowerCmd.Apply<WeakPower>(choiceContext, target, weakStacks, applier, this);
		await SplitDebuff<VulnerablePower, SplitVulnerablePower>(choiceContext, target, applier, this);
		await SplitDebuff<WeakPower, SplitWeakPower>(choiceContext, target, applier, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Vulnerable.UpgradeValueBy(2m);
		base.DynamicVars.Weak.UpgradeValueBy(2m);
	}

	/// <summary>易伤/类易伤、虚弱/类虚弱各自独立平分；单条 debuff 仅 1 层时不分割。</summary>
	private static async Task SplitDebuff<TOriginal, TSplit>(
		PlayerChoiceContext choiceContext,
		Creature target,
		Creature applier,
		CardModel? cardSource)
		where TOriginal : PowerModel
		where TSplit : PowerModel
	{
		await SplitOneSide<TOriginal, TSplit>(choiceContext, target, applier, cardSource);
		await SplitOneSide<TSplit, TSplit>(choiceContext, target, applier, cardSource);
	}

	private static async Task SplitOneSide<TFrom, TTo>(
		PlayerChoiceContext choiceContext,
		Creature target,
		Creature applier,
		CardModel? cardSource)
		where TFrom : PowerModel
		where TTo : PowerModel
	{
		int amount = target.GetPowerAmount<TFrom>();
		if (amount < 2)
		{
			return;
		}

		int kept = (amount + 1) / 2;
		int removed = amount / 2;

		TFrom? from = target.GetPower<TFrom>();
		if (from == null)
		{
			return;
		}

		await PowerCmd.Remove(from);
		if (kept > 0)
		{
			await PowerCmd.Apply<TFrom>(choiceContext, target, kept, applier, cardSource);
		}

		if (removed > 0)
		{
			await PowerCmd.Apply<TTo>(choiceContext, target, removed, applier, cardSource);
		}
	}
}
