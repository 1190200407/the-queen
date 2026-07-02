using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(TokenCardPool))]
public sealed class FirstChant : QueenCardModel
{
	private const int energyCost = 13;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Token;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = false;

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [ModKeywordRegistry.GetCardKeyword(QueenKeyword.Fade)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
		HoverTipFactory.Static(StaticHoverTip.ReplayStatic),
		.. HoverTipFactory.FromAffliction<Bound>()
	];

	internal override bool HasSelfBound => true;

	public FirstChant()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.Owner.PlayerCombatState == null)
		{
			return;
		}

		bool showFullVfx = cardPlay.IsFirstInSeries;
		foreach (CardModel card in base.Owner.PlayerCombatState.Hand.Cards.ToList())
		{
			if (card is SecondChant second)
			{
				second.BaseReplayCount += 1;
				await MysticChantStrengthenVfx.PlayAfterStrengthen(second, showFullVfx);
			}
		}
	}

    protected override PileType GetResultPileType()
    {
		PileType resultPileType = base.GetResultPileType();
		if (resultPileType != PileType.Discard && resultPileType != PileType.Exhaust)
		{
			return resultPileType;
		}
		return PileType.Hand;
    }
}
