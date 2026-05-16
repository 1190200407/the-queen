using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>产卵：召唤10。学习意图：生成4张结实的卵。魂缚</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class LayEggs : LearnIntentCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private const decimal summon = 10m;
    private const decimal eggCount = 4m;

    public override int MaxUpgradeLevel => 0;

    protected override bool ShouldSummonBeforeLearnIntent => true;

    internal override bool HasSelfBound => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(summon).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new IntVar("EggCount", eggCount),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ..HoverTipFactory.FromAffliction<Bound>(),
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromCard<ToughEgg>(),
        HoverTipFactory.FromCard<Nibble>()
    ];

    public LayEggs()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>(
        [
            new AmalgamGenerateCardIntentAction<ToughEgg>(eggCount),
        ]);
    }
}

