using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(EnemyCardPool))]
public sealed class Bite : LearnIntentCardModel
{
    private const decimal summon = 3m;
    private const decimal learnIntentDamage = 8m;
    private const decimal learnIntentBlock = 8m;
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(summon).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentDamageVar(learnIntentDamage, ValueProp.Move),
        new AmalgamLearnIntentBlockVar(learnIntentBlock),
    ];

    public Bite()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        CompositeKey = AmalgamCompositeKey.Bowlbug;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);

        decimal damage = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        decimal block = base.DynamicVars["LearnIntentBlock"].BaseValue;

        await ApplyLearnOrCombineIntentAsync(choiceContext, new AmalgamOffenseIntentAction(damage));
        await ApplyLearnOrCombineIntentAsync(choiceContext, new AmalgamGainBlockIntentAction(block));
    }
}
