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

/// <summary>剧毒唾液：召唤；<see cref="FriendlyAmalgamCmd.CombineIntent"/> 学习虚弱（与盛碗虫系共用 <see cref="Headbutt.BowlbugRockCompositeKey"/>）。消耗。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class ToxicSpit : QueenCardModel
{
    private const decimal summon = 3m;
    private const decimal learnIntentWeak = 2m;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, QueenKeyword.amalgamComposite];

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(summon).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentWeakVar(learnIntentWeak),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(QueenKeyword.amalgamComposite),
        HoverTipFactory.FromPower<WeakPower>(),
    ];

    public ToxicSpit()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);

        decimal weak = base.DynamicVars["LearnIntentWeak"].BaseValue;
        await FriendlyAmalgamCmd.CombineIntent(
            choiceContext,
            base.Owner,
            new AmalgamApplyWeakIntentAction(weak),
            this,
            Headbutt.BowlbugRockCompositeKey);
    }
}
