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
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>裂隙抓挠：抓挠系，可受其他抓挠的全局伤害加成；打出时不提供抓挠全局强化。</summary>
[RegisterCard(typeof(QueenCardPool))]
public sealed class RiftScratch : ScratchTaggedCard
{
	private const int energyCost = 1;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(7m, ValueProp.Move),
		new RepeatVar(1),
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		.. HoverTipFactory.FromAffliction<Bound>(),
		HoverTipFactory.FromPower<SoulLampPower>(),
	];

	internal override bool HasSelfBound => true;

	public RiftScratch()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		_ = choiceContext;
		_ = target;
		if (base.IsClone || !ReferenceEquals(cardSource, this) || base.Owner?.Creature is null)
		{
			return;
		}

		if (dealer != base.Owner.Creature || !props.IsPoweredAttack() || result.UnblockedDamage <= 0)
		{
			return;
		}

		await QueenCardCmd.AddSoulLamp(choiceContext, base.Owner, 1);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.WithHitCount(base.DynamicVars.Repeat.IntValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}
}
