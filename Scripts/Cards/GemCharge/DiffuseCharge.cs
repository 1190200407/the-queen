using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

[Pool(typeof(TokenCardPool))]
public sealed class DiffuseCharge : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Token;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = false;

	internal override bool UseBoundAfflictionOverlayForPreview => true;
	public override IEnumerable<CardKeyword> CanonicalKeywords => [QueenKeyword.fade];
	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DexterityPower>(1m)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<DexterityPower>(),
		HoverTipFactory.FromKeyword(QueenKeyword.fade)
	];

	public DiffuseCharge()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<DexterityPower>(base.Owner.Creature, base.DynamicVars.Dexterity.BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Dexterity.UpgradeValueBy(1m);
	}
}
