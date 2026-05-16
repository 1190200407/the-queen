using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>缩小射线：学习意图，对聚合体以外所有单位施加 1 回合缩小，并带魂缚。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class Shrinker : LearnIntentCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("LearnIntentShrink", 1m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<ShrinkPower>(),
        ..HoverTipFactory.FromAffliction<Bound>(),
    ];

    internal override bool HasSelfBound => true;
    public override int MaxUpgradeLevel => 0;

    public Shrinker()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal turns = base.DynamicVars["LearnIntentShrink"].BaseValue;
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([AmalgamActionRegistry.CreateShrinkRay(turns)]);
    }
}

