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

/// <summary>翱翔：学习意图为获得翱翔；消耗。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class Soar : LearnIntentCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private const decimal summon = 5m;
    private const decimal soarStacks = 1m;
    private const string soarEntryId = "AMALGAM_SOAR_POWER";

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(summon).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new PowerVar<AmalgamSoarPower>(soarStacks),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<AmalgamSoarPower>(),
    ];

    protected override bool ShouldSummonBeforeLearnIntent => true;

    public Soar()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal stacks = base.DynamicVars.Power<AmalgamSoarPower>().BaseValue;
        if (stacks <= 0m)
        {
            return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([]);
        }

        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>(
            [new AmalgamGainBuffIntentAction<AmalgamSoarPower>(stacks, soarEntryId)]);
    }
}
