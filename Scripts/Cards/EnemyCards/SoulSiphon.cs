using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>灵魂虹吸：召唤并学习「全体敌人失去力�?+ 你获得力量与敏捷」�?/summary>
[Pool(typeof(EnemyCardPool))]
public sealed class SoulSiphon : LearnIntentCardModel
{
private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(15m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new IntVar("Stacks", 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    protected override bool ShouldSummonBeforeLearnIntent => true;
    public override int MaxUpgradeLevel => 0;

    public SoulSiphon()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([new AmalgamSoulSiphonIntentAction(base.DynamicVars["Stacks"].BaseValue)]);
    }
}

