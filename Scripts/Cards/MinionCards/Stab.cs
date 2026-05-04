using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>刺击：衍生牌（输出型），学习意图为伤害并使敌人本回合失去力量；消逝。</summary>
[Pool(typeof(TokenCardPool))]
public sealed class Stab : LearnIntentCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;
    private const decimal learnIntentDamage = 11m;
    private const decimal learnIntentStrengthLoss = 2m;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [QueenKeyword.fade];

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new AmalgamLearnIntentDamageVar(learnIntentDamage, ValueProp.Move),
        new IntVar("LearnIntentStrengthLoss", learnIntentStrengthLoss),
    ];

    public Stab()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal dmg = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        decimal strLoss = base.DynamicVars["LearnIntentStrengthLoss"].BaseValue;
        AmalgamActionModel? intent = new AmalgamAttackAndStrengthDownIntentAction(dmg, strLoss);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}

