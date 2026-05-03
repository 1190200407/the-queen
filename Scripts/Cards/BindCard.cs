using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class BindCard : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Common;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CardKeyword.Retain),
		.. HoverTipFactory.FromAffliction<Bound>()
	];

	public BindCard()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		IEnumerable<CardModel> selected = await CardSelectCmd.FromHand(
			choiceContext,
			base.Owner,
			new CardSelectorPrefs(new LocString("cards", "COMICCHESS-BIND_CARD.selectionPrompt"), 1),
			c => c.Affliction is not Bound,
			this
		);

		CardModel? card = selected.FirstOrDefault();
		if (card is null)
		{
			return;
		}
		CardCmd.ClearAffliction(card);
		await CardCmd.Afflict<Bound>(card, 1m);
	}

    protected override void OnUpgrade()
    {
		base.EnergyCost.UpgradeBy(-1);
    }
}
