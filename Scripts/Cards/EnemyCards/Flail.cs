using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>连枷：召唤并学习 10×2 的多段进攻意图。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class Flail : LearnIntentCardModel
{
    private const decimal learnIntentDamagePerHit = 10m;
    private const int learnIntentHitCount = 2;
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(10m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentDamageVar(learnIntentDamagePerHit, ValueProp.Move),
    ];

    protected override bool ShouldSummonBeforeLearnIntent => true;

    public override int MaxUpgradeLevel => 0;

    public Flail()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal damagePerHit = base.DynamicVars["LearnIntentDamage"].BaseValue;
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateOffenseMulti(damagePerHit, learnIntentHitCount);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}

