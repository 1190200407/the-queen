using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;


[RegisterCard(typeof(QueenCardPool))]
public sealed class Reminisce : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Pick", 1m)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<SoulLampPower>(), ..HoverTipFactory.FromAffliction<Bound>()];

	public Reminisce()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int wantPick = base.DynamicVars["Pick"].IntValue;
		CardPile exhaust = PileType.Exhaust.GetPile(base.Owner);
		List<CardModel> boundInExhaust = exhaust.Cards.Where(c => c.Affliction is Bound).ToList();
		if (boundInExhaust.Count > 0)
		{
			int pick = Math.Min(wantPick, boundInExhaust.Count);
			CardSelectorPrefs prefs = new(
				new LocString("cards", "STS2_COMICCHESS_THEQUEEN_CARD_REMINISCE.selectionPrompt"),
				pick,
				pick
			);
			foreach (CardModel card in await CardSelectCmd.FromSimpleGrid(choiceContext, boundInExhaust, base.Owner, prefs))
			{
				await CardPileCmd.Add(card, PileType.Hand);
			}
		}

		await QueenCardCmd.AddSoulLamp(base.Owner);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["Pick"].UpgradeValueBy(1m);
	}
}
