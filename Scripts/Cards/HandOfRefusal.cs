using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(TokenCardPool))]
public sealed class HandOfRefusal : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Token;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = false;

	public override bool GainsBlock => true;

	public override IEnumerable<CardKeyword> CanonicalKeywords =>
		[ModKeywordRegistry.GetCardKeyword(QueenKeyword.Fade)];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade),
		HoverTipFactory.FromCard<HandOfRefusal>(upgrade: base.IsUpgraded),
		HoverTipFactory.FromCard<HandOfSeizure>(upgrade: base.IsUpgraded),
		.. HoverTipFactory.FromAffliction<Bound>(),
	];

	internal override bool HasSelfBound => true;

	public HandOfRefusal()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		await HundredHandsBanquet.CreateInHandInternal(base.Owner, base.CombatState, base.IsUpgraded);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(3m);
	}
}
