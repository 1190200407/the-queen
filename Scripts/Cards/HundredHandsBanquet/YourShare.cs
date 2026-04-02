using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

[Pool(typeof(TokenCardPool))]
public sealed class YourShare : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Token;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = false;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [QueenKeyword.fade];

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(6m, ValueProp.Move),
		new IntVar("HpLossOnExhaust", 2m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(QueenKeyword.fade),
		.. HoverTipFactory.FromAffliction<Bound>()
	];

	internal override bool UseBoundAfflictionOverlayForPreview => true;

	public YourShare()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		if (card != this || base.Owner?.Creature == null)
		{
			return;
		}

		await CreatureCmd.Damage(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["HpLossOnExhaust"].BaseValue,
			ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
			this
		);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Target == null)
		{
			return;
		}

		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);

		await CardPileCmd.Draw(choiceContext, 1m, base.Owner);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(3m);
		base.DynamicVars["HpLossOnExhaust"].UpgradeValueBy(-1m);
	}
}
