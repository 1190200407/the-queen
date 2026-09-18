using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>未登记怪物的普通战捕获占位：1 费消耗，召唤 5。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class UnknownSoul : UnknownSoulCardModel
{
    public UnknownSoul()
        : base(energyCost: 1, CardRarity.Common, summon: 5m)
    {
    }
}

/// <summary>未登记怪物的精英战捕获占位：2 费消耗，召唤 15。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class UnknownSoulUncommon : UnknownSoulCardModel
{
    public UnknownSoulUncommon()
        : base(energyCost: 2, CardRarity.Uncommon, summon: 15m)
    {
    }
}

/// <summary>未登记怪物的首领战捕获占位：3 费消耗，召唤 25。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class UnknownSoulRare : UnknownSoulCardModel
{
    public UnknownSoulRare()
        : base(energyCost: 3, CardRarity.Rare, summon: 25m)
    {
    }
}

public abstract class UnknownSoulCardModel : QueenCardModel
{
    private readonly decimal _summon;

    protected UnknownSoulCardModel(int energyCost, CardRarity rarity, decimal summon)
        : base(energyCost, CardType.Skill, rarity, TargetType.Self, shouldShowInCardLibrary: false)
    {
        _summon = summon;
    }

    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(_summon).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
    ];

    public override bool ShouldAddToDeck(CardModel card) => card is not UnknownSoulCardModel;

    public override async Task AfterAddToDeckPrevented(CardModel card)
    {
        CardModel? replacement = CreateRandomMonsterCard(card.Owner, Rarity);
        if (replacement != null)
        {
            await CardPileCmd.Add(replacement, PileType.Deck);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);
    }

    private static CardModel? CreateRandomMonsterCard(Player player, CardRarity rarity)
    {
        if (player.RunState is not { } runState)
        {
            return null;
        }

        CardPoolModel pool = ModelDb.CardPool<EnemyCardPool>();
        List<CardModel> candidates = pool
            .GetUnlockedCards(player.UnlockState, runState.CardMultiplayerConstraint)
            .Where(card => card.Rarity == rarity && card.CanBeGeneratedInCombat)
            .ToList();
        if (candidates.Count == 0)
        {
            return null;
        }

        CardCreationOptions options = new CardCreationOptions(
                [pool],
                CardCreationSource.Other,
                CardRarityOddsType.Uniform,
                card => candidates.Any(candidate => candidate.Id == card.Id))
            .WithFlags(CardCreationFlags.NoModifyHooks | CardCreationFlags.NoCardPoolModifications);

        return CardFactory.CreateForReward(player, 1, options).FirstOrDefault()?.Card;
    }
}
