using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>噪音：衍生牌（防御型），学习意图为抽牌并附魔晕眩；消逝�?/summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class Noise : LearnIntentCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;
    private const decimal drawNow = 1m;
    private const decimal drawNextTurn = 1m;

    protected override IEnumerable<string> RegisteredKeywordIds => [QueenKeyword.Fade];

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar((int)(drawNow + drawNextTurn)),
    ];

    public Noise()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        AmalgamActionModel? intent = new AmalgamNoiseIntentAction(drawNow, drawNextTurn);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}

