using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>未登记怪物的普通战捕获占位：拾起时替换为一张普通怪物卡。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class UnknownSoul : UnknownSoulCardModel
{
    public UnknownSoul()
        : base(CardRarity.Common)
    {
    }
}

/// <summary>未登记怪物的精英战捕获占位：拾起时替换为一张罕见怪物卡。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class UnknownSoulUncommon : UnknownSoulCardModel
{
    public UnknownSoulUncommon()
        : base(CardRarity.Uncommon)
    {
    }
}

/// <summary>未登记怪物的首领战捕获占位：拾起时替换为一张稀有怪物卡。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class UnknownSoulRare : UnknownSoulCardModel
{
    public UnknownSoulRare()
        : base(CardRarity.Rare)
    {
    }
}

public abstract class UnknownSoulCardModel : QueenCardModel
{
    protected UnknownSoulCardModel(CardRarity rarity)
        : base(-1, CardType.Skill, rarity, TargetType.Self, shouldShowInCardLibrary: false)
    {
    }

    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedInCombat => false;

    public override bool ShouldAddToDeck(CardModel card) => card is not UnknownSoulCardModel;

    public override async Task AfterAddToDeckPrevented(CardModel card)
    {
        CardModel? replacement = CreateRandomMonsterCard(card.Owner, Rarity);
        if (replacement != null)
        {
            await new SpecialCardReward(replacement, card.Owner).SelectUnsynchronized();
        }
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
