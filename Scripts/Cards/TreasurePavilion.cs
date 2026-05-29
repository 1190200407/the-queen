using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>藏宝阁：从弃牌堆取回 1 张非魂缚牌并侵蚀为魂缚，获得 1 点魂灯�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class TreasurePavilion : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon; // 中文“罕见�? Uncommon
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ..base.AdditionalHoverTips,
        ..HoverTipFactory.FromAffliction<Bound>(),
        HoverTipFactory.FromPower<SoulLampPower>(),
    ];

    public TreasurePavilion()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        CardPile discard = PileType.Discard.GetPile(base.Owner);
        List<CardModel> candidates = discard.Cards
            .Where(c => c.Affliction is not Bound)
            .ToList();

        if (candidates.Count > 0)
        {
            CardSelectorPrefs prefs = new(
                new LocString("cards", "THE_QUEEN_CARD_TREASURE_PAVILION.selectionPrompt"),
                1,
                1);

            CardModel? picked = (await CardSelectCmd.FromSimpleGrid(choiceContext, candidates, base.Owner, prefs)).FirstOrDefault();
            if (picked is not null)
            {
                await CardPileCmd.Add(picked, PileType.Hand);
                CardCmd.ClearAffliction(picked);
                await CardCmd.Afflict<Bound>(picked, 1m);
            }
        }

        await QueenCardCmd.AddSoulLamp(choiceContext, base.Owner);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

