using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

/// <summary>刺耳尖啸：召唤并学习「目标本回合失去力量 + 生成一张刺耳尖啸」�?/summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class Screech : LearnIntentCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ModKeywordRegistry.GetCardKeyword(QueenKeyword.Fade)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(5m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new IntVar("LearnIntentStrengthLoss", 3m),
        new IntVar("LearnIntentGenerate", 1m),
    ];

    protected override bool ShouldSummonBeforeLearnIntent => true;
    public override int MaxUpgradeLevel => 0;

    public Screech()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        decimal loss = base.DynamicVars["LearnIntentStrengthLoss"].BaseValue;
        decimal count = base.DynamicVars["LearnIntentGenerate"].BaseValue;
        AmalgamActionModel? action = loss > 0m && count > 0m
            ? new AmalgamStrengthDownAndGenerateCardIntentAction<Screech>(loss, count)
            : null;
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([action]);
    }
}

