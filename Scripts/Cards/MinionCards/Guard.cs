using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>格挡：衍生牌（防御型），学习意图为获得格挡；消逝�?/summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class Guard : LearnIntentCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;
    private const decimal learnIntentBlock = 10m;

    protected override IEnumerable<string> RegisteredKeywordIds => [QueenKeyword.Fade];

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new AmalgamLearnIntentBlockVar(learnIntentBlock),
    ];

    public Guard()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal block = base.DynamicVars["LearnIntentBlock"].BaseValue;
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateBlock(block);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}

