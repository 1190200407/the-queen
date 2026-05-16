using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>暮星：抽牌；魂缚；消逝。</summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class EveningStar : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Token;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = false;

	public override int MaxUpgradeLevel => 0;

	internal override bool HasSelfBound => true;

	protected override IEnumerable<string> RegisteredKeywordIds => [QueenKeyword.Fade];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Draw", 2m)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		.. HoverTipFactory.FromAffliction<Bound>(),
	];

	public EveningStar()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = cardPlay;
		await CardPileCmd.Draw(choiceContext, base.DynamicVars["Draw"].BaseValue, base.Owner);
	}
}
