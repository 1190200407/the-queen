using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>同族小队！集结！（亲族祭司）：召唤并生成 token，然后学习两个意图。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class TheKins : LearnIntentCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private const decimal generateCount = 2m;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(15m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new IntVar("GenerateCount", generateCount),

        new AmalgamLearnIntentDamageVar(11m, ValueProp.Move),
        new IntVar("LearnIntentWeak", 2m),

        new AmalgamLearnIntentDamageVar("LearnIntentDamage2", 11m, ValueProp.Move),
        new IntVar("LearnIntentStrengthLoss", 2m),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        HoverTipFactory.FromCard<PowerDance>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    public override int MaxUpgradeLevel => 0;

    protected override bool ShouldSummonBeforeLearnIntent => true;

    public TheKins()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task AfterSummonBeforeLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (base.Owner.Creature.CombatState is not { } combatState)
        {
            return;
        }

        int count = (int)base.DynamicVars["GenerateCount"].BaseValue;
        for (int i = 0; i < count; i++)
        {
            await QueenCardCmd.CreateInHand<PowerDance>(base.Owner, combatState, isUpgraded: false);
        }
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (base.Owner.Creature.CombatState is not { } combatState)
        {
            return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([]);
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is not { IsAlive: true })
        {
            return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([]);
        }

        decimal dmg1 = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        decimal weak = base.DynamicVars["LearnIntentWeak"].BaseValue;
        AmalgamActionModel? intent1 = AmalgamActionRegistry.CreateAttackAndWeak(dmg1, weak);

        decimal dmg2 = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage2");
        decimal strLoss = base.DynamicVars["LearnIntentStrengthLoss"].BaseValue;
        AmalgamActionModel? intent2 = new AmalgamAttackAndLoseStrengthIntentAction(dmg2, strLoss);

        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent1, intent2]);
    }
}
