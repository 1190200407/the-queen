using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

/// <summary>力量之舞：爪�?token，学习意图为获得力量�?/summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class PowerDance : LearnIntentCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    private const decimal learnIntentStrength = 2m;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ModKeywordRegistry.GetCardKeyword(QueenKeyword.Fade)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("LearnIntentStrength", learnIntentStrength),
    ];

    public override int MaxUpgradeLevel => 0;

    public PowerDance()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        //CompositeKey = AmalgamCompositeKey.Kin;
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal strength = base.DynamicVars["LearnIntentStrength"].BaseValue;
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateStrength(strength);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}

