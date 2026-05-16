using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>甩动：召唤后学习多段攻击意图（4伤害×4）。可被感染反复附魔叠层。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class Lash : LearnIntentCardModel
{
    private const decimal summon = 4m;
    private const decimal learnIntentDamagePerHit = 4m;
    private const int learnIntentHitCount = 4;
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [..HoverTipFactory.FromEnchantment<Infested>()];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override bool ShouldSummonBeforeLearnIntent => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(summon).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentDamageVar(learnIntentDamagePerHit, ValueProp.Move),
    ];

    public override int MaxUpgradeLevel => 0;

    public Lash()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal damagePerHit = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateOffenseMulti(damagePerHit, learnIntentHitCount);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}

