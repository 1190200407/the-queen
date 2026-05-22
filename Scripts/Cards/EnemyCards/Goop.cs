using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>粘液：召唤并学习意图为抽牌并附魔（黏液）。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class Goop : LearnIntentCardModel
{
    private const decimal learnIntentDraw = 1m;
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(5m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new IntVar("LearnIntentDraw", learnIntentDraw),
    ];

    protected override bool ShouldSummonBeforeLearnIntent => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [QueenHoverTips.LearnIntent, ..HoverTipFactory.FromEnchantment<Slimed>()];

    public override int MaxUpgradeLevel => 0;

    public Goop()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal draw = base.DynamicVars["LearnIntentDraw"].BaseValue;
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateDrawAndEnchant<Slimed>(draw);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}

