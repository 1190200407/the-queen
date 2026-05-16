using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

/// <summary>增益（蜜虫）：召唤；<see cref="FriendlyAmalgamCmd.CombineIntent"/> 学习聚合体获得力量（与盛碗虫系共用 <see cref="Headbutt.BowlbugRockCompositeKey"/>）。消耗。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class Buff : LearnIntentCardModel
{
    private const decimal summon = 3m;
    private const decimal learnIntentStrength = 1m;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<string> RegisteredKeywordIds => [QueenKeyword.AmalgamComposite];
    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(summon).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentStrengthVar(learnIntentStrength),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        QueenHoverTips.LearnIntent,
        ModKeywordRegistry.CreateHoverTip(QueenKeyword.AmalgamComposite),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    public Buff()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);

        decimal strength = base.DynamicVars["LearnIntentStrength"].BaseValue;
        await FriendlyAmalgamCmd.CombineIntent(
            choiceContext,
            base.Owner,
            new AmalgamGainStrengthIntentAction(strength),
            this,
            Headbutt.BowlbugRockCompositeKey);
    }
}
