using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>火焰喷射器：召唤并学习「抽牌附魔（灼烧）+ 下回合伤害」意图；消耗。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class Flamethrower : LearnIntentCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(15m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new IntVar("LearnIntentDraw", 4m),
        new IntVar("LearnIntentBurn", 3m),
        new AmalgamLearnIntentDamageVar(40m, ValueProp.Move),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        ..HoverTipFactory.FromEnchantment<Burn>(3),
    ];

    protected override bool ShouldSummonBeforeLearnIntent => true;

    public override int MaxUpgradeLevel => 0;

    public Flamethrower()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal draw = base.DynamicVars["LearnIntentDraw"].BaseValue;
        decimal burn = base.DynamicVars["LearnIntentBurn"].BaseValue;
        decimal dmg = base.DynamicVars["LearnIntentDamage"].BaseValue;
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>(
        [
            new AmalgamFlamethrowerIntentAction(draw, burn, dmg),
        ]);
    }
}

