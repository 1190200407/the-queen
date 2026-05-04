using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>撕咬：召唤；分两次 <see cref="FriendlyAmalgamCmd.CombineIntent"/> 学习同键的聚合伤害与格挡（同一灯槽内为两次行动）。消耗。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class Bite : QueenCardModel
{
    public const string BowlbugEggCompositeKey = "BOWLBUG";

    private const decimal summon = 3m;
    private const decimal learnIntentDamage = 8m;
    private const decimal learnIntentBlock = 8m;
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, QueenKeyword.amalgamComposite];

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(summon).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentDamageVar(learnIntentDamage, ValueProp.Move),
        new AmalgamLearnIntentBlockVar(learnIntentBlock),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(QueenKeyword.amalgamComposite),
    ];

    public Bite()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);

        decimal damage = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        decimal block = base.DynamicVars["LearnIntentBlock"].BaseValue;

        await FriendlyAmalgamCmd.CombineIntent(
            choiceContext,
            base.Owner,
            new AmalgamOffenseIntentAction(damage),
            this,
            BowlbugEggCompositeKey);

        await FriendlyAmalgamCmd.CombineIntent(
            choiceContext,
            base.Owner,
            new AmalgamGainBlockIntentAction(block),
            this,
            BowlbugEggCompositeKey);
    }
}
