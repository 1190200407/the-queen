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

/// <summary>易伤孢子：召唤；学习意图为通用 <see cref="AmalgamApplyVulnerableIntentAction"/>（施加易伤）。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class FrailSpores : LearnIntentCardModel
{
    private const decimal learnIntentVulnerable = 3m;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(5m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentVulnerableVar(learnIntentVulnerable),
    ];
    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];
    protected override bool ShouldSummonBeforeLearnIntent => true;

    public FrailSpores()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal stacks = base.DynamicVars["LearnIntentVulnerable"].BaseValue;
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateVulnerable(stacks);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }

}
