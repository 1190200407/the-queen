using System.Collections.Generic;
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
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class Taste : QueenCardModel
{
	private const int energyCost = 2;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromCard<Devour>(base.IsUpgraded),
		QueenHoverTips.SoulLamp
	];

	public Taste()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.CombatState == null || base.Owner == null)
		{
			return;
		}

		IEnumerable<CardModel> selected = await CardSelectCmd.FromHand(
			choiceContext,
			base.Owner,
			new CardSelectorPrefs(new LocString("cards", "COMICCHESS-TASTE.selectionPrompt"), 2),
			c => c != this,
			this);

		foreach (CardModel card in selected)
		{
			CardPileAddResult? result = await CardCmd.TransformTo<Devour>(card, CardPreviewStyle.None);
			if (result != null && result.Value.cardAdded is CardModel added && added.Affliction is not Bound)
			{
				await CardCmd.Afflict<Bound>(added, 1m);
			}
			if (base.IsUpgraded && result != null && result.Value.cardAdded != null)
			{
				CardCmd.Upgrade(result.Value.cardAdded);
			}
		}

		await QueenCardCmd.AddSoulLamp(base.Owner, 2);
	}
}
