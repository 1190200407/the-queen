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

/// <summary>剧毒：召唤并学习抽牌附魔（毒素）。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class ToxicCard : LearnIntentCardModel
{
    private const decimal learnIntentDraw = 2m;
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(7m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new IntVar("LearnIntentDraw", learnIntentDraw),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        ..HoverTipFactory.FromEnchantment<Toxic>(),
    ];

    public override int MaxUpgradeLevel => 0;

    protected override bool ShouldSummonBeforeLearnIntent => true;

    public ToxicCard()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal draw = base.DynamicVars["LearnIntentDraw"].BaseValue;
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateDrawAndEnchant<Toxic>(draw);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}
