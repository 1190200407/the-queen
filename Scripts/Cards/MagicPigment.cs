using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Random;

namespace ComicChess.TheQueen;

/// <summary>神奇颜料：生成若干张随机颜色牌（卡池同 <see cref="HexCursePower"/>）并侵蚀为魂缚，获得魂灯。消耗。</summary>
[Pool(typeof(QueenCardPool))]
public sealed class MagicPigment : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..HoverTipFactory.FromAffliction<Bound>(),
        QueenHoverTips.SoulLamp,
    ];

    public MagicPigment()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        if (base.Owner.RunState is null || base.CombatState is null)
        {
            return;
        }

        int count = base.DynamicVars.Cards.IntValue;
        List<CardModel> allUnlocked = base.Owner.UnlockState.CharacterCardPools
            .SelectMany(p => p.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint))
            .Where(static c => c.Rarity is not (CardRarity.Basic or CardRarity.Ancient))
            .ToList();

        if (allUnlocked.Count == 0)
        {
            return;
        }

        Rng rng = base.Owner.RunState.Rng.CombatCardGeneration;
        int pick = count < allUnlocked.Count ? count : allUnlocked.Count;
        foreach (CardModel card in CardFactory.GetDistinctForCombat(base.Owner, allUnlocked, pick, rng))
        {
            if (base.IsUpgraded)
            {
                CardCmd.Upgrade(card);
            }
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);
            CardCmd.ClearAffliction(card);
            await CardCmd.Afflict<Bound>(card, 1m);
        }

        await QueenCardCmd.AddSoulLamp(base.Owner, 2);
    }
}
