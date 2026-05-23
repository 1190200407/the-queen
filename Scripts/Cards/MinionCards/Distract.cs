using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

/// <summary>干扰：学习意图为抽牌�?/summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class Distract : LearnIntentCardModel
{
    private const decimal learnIntentDraw = 2m;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ModKeywordRegistry.GetCardKeyword(QueenKeyword.Fade)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];

    public override int MaxUpgradeLevel => 0;

    public Distract()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal draw = base.DynamicVars.Cards.IntValue;
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateDrawAndEnchant<Dazed>(draw);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}
