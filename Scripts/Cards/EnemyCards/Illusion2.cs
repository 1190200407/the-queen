using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

/// <summary>幻象Ⅱ：召唤并在手牌中生成猛击。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class Illusion2 : LearnIntentCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(7m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        HoverTipFactory.FromCard<Slam>(),
    ];

    protected override bool ShouldSummonBeforeLearnIntent => true;

    public override int MaxUpgradeLevel => 0;

    public Illusion2()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateGenerateCard<Slam>(1m);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}
