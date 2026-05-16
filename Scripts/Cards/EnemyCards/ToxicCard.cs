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

/// <summary>剧毒：召唤并学习抽牌附魔（毒素）。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class ToxicCard : LearnIntentCardModel
{
    private const decimal learnIntentDraw = 1m;
    private const int learnIntentEnchantAmount = 5;

    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(7m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new IntVar("LearnIntentDraw", learnIntentDraw),
        new IntVar("LearnIntentEnchantAmount", learnIntentEnchantAmount),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ..base.AdditionalHoverTips,
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
        decimal enchantAmount = base.DynamicVars["LearnIntentEnchantAmount"].BaseValue;
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateDrawAndEnchantWithAmount<Toxic>(draw, enchantAmount);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}
