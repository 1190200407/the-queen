using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class Scratch : ScratchTaggedCard
{
	private const int energyCost = 1;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Common;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(5m, ValueProp.Move),
		new RepeatVar(1),
		new IntVar("IncreaseDamage", 2m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		.. HoverTipFactory.FromAffliction<Bound>()
	];

	internal override bool UseBoundAfflictionOverlayForPreview => true;

	public Scratch()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	public override async Task BeforeCombatStart()
	{
		if (base.Owner?.Creature?.CombatState == null)
		{
			return;
		}
		if (Affliction is not Bound)
		{
			await CardCmd.Afflict<Bound>(this, 1m);
		}
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

		ArgumentNullException.ThrowIfNull(base.Owner);
		ArgumentNullException.ThrowIfNull(base.Owner.PlayerCombatState);
		decimal increase = base.DynamicVars["IncreaseDamage"].BaseValue;
		foreach (ScratchTaggedCard card in base.Owner.PlayerCombatState.AllCards.OfType<ScratchTaggedCard>())
		{
			card.BuffFromScratchPlay(increase);
		}

		QueenScratchBonusTracker.RecordScratchPlay(base.Owner, increase);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
		base.DynamicVars["IncreaseDamage"].BaseValue += 1m;
	}
}
