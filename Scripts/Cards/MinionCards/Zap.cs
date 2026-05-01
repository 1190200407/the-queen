using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>电击：衍生牌（输出型），学习意图为单体伤害；消逝。</summary>
[Pool(typeof(TokenCardPool))]
public sealed class Zap : LearnIntentCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;
    private const decimal learnIntentDamage = 14m;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [QueenKeyword.fade];

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new AmalgamLearnIntentDamageVar(learnIntentDamage, ValueProp.Move),
    ];

    public Zap()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal dmg = base.DynamicVars["LearnIntentDamage"].BaseValue;
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateOffense(dmg);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}

