using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

[Pool(typeof(TokenCardPool))]
public sealed class SecondChant : QueenCardModel
{
	private const int energyCost = 13;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Token;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = false;

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [QueenKeyword.fade];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [.. HoverTipFactory.FromAffliction<Bound>()];

	internal override bool HasSelfBound => true;

	public SecondChant()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.Owner.PlayerCombatState == null)
		{
			return;
		}

		foreach (CardModel card in base.Owner.PlayerCombatState.Hand.Cards.ToList())
		{
			if (card is FinalChant)
			{
				card.DynamicVars["Repeat"].BaseValue += 1m;
				await MysticChantStrengthenVfx.PlayAfterStrengthen(card);
			}
		}
	}

	protected override PileType GetResultPileType() => PileType.Hand;
}
