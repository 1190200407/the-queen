using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>重拳出击：召唤并学习「格挡」与「2连击 + 虚弱」意图。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class PunchOff : LearnIntentCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    private const int hitCount = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(10m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentBlockVar(10m),
        new AmalgamLearnIntentDamageVar(5m, ValueProp.Move),
        new AmalgamLearnIntentWeakVar(1m),
    ];

    protected override bool ShouldSummonBeforeLearnIntent => true;
    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<WeakPower>(),
    ];

    public PunchOff()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal block = base.DynamicVars["LearnIntentBlock"].BaseValue;
        decimal dmg = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        decimal weak = base.DynamicVars["LearnIntentWeak"].BaseValue;
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>(
        [
            AmalgamActionRegistry.CreateBlock(block),
            AmalgamActionRegistry.CreateOffenseMultiAndWeak(dmg, hitCount, weak),
        ]);
    }
}

