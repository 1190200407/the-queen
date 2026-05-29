using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.CardTags;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(QueenCardPool))]
public sealed class QuickAdaptation : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Pick", 1m)];

    public QuickAdaptation()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        CardPile drawPile = PileType.Draw.GetPile(base.Owner);
        List<CardModel> candidates = drawPile.Cards
            .Where(static c => c is LearnIntentCardModel) 
            .ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        int pick = Math.Min(base.DynamicVars["Pick"].IntValue, candidates.Count);
        if (pick <= 0)
        {
            return;
        }

        CardSelectorPrefs prefs = new(
            new LocString("cards", "STS2_COMICCHESS_THEQUEEN_CARD_QUICK_ADAPTATION.selectionPrompt"),
            pick,
            pick);
        IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGrid(choiceContext, candidates, base.Owner, prefs);
        foreach (CardModel card in selected.ToList())
        {
            await CardAutoPlayDirect.AutoPlayAsync(
                choiceContext,
                card,
                target: null,
                AutoPlayType.Default);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Pick"].UpgradeValueBy(1m);
    }
}
