using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
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
        : base(energyCost, CardType.Skill, rarity, TargetType.Self, shouldShowInCardLibrary: true)
    {
        _summon = summon;
    }

    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(_summon).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);
    }
}
