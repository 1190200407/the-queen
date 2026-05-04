using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>连续冲击：学习「攻击+力量」以及「2 连击攻击」两段意图。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class DoubleBlast : LearnIntentCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    private const int secondHitCount = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new AmalgamLearnIntentDamageVar("LearnIntentDamage", 6m, ValueProp.Move),
        new AmalgamLearnIntentStrengthVar(1m),
        new AmalgamLearnIntentDamageVar("LearnIntentDamage2", 5m, ValueProp.Move),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    public override int MaxUpgradeLevel => 0;

    public DoubleBlast()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal dmg1 = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        decimal str = base.DynamicVars["LearnIntentStrength"].BaseValue;
        decimal dmg2 = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage2");
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>
        ([
            AmalgamActionRegistry.CreateAttackAndStrength(dmg1, str),
            AmalgamActionRegistry.CreateOffenseMulti(dmg2, secondHitCount),
        ]);
    }
}

