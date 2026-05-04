using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>能量球：学习「进攻 + 力量」双意图，并带魂缚。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class EnergyOrb : LearnIntentCardModel
{
    private const decimal learnIntentDamage = 4m;
    private const decimal learnIntentStrength = 2m;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new AmalgamLearnIntentDamageVar(learnIntentDamage, ValueProp.Move),
        new AmalgamLearnIntentStrengthVar(learnIntentStrength),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    internal override bool HasSelfBound => true;
    public override int MaxUpgradeLevel => 0;

    public EnergyOrb()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal damage = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        decimal strength = base.DynamicVars["LearnIntentStrength"].BaseValue;
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>
        ([
            AmalgamActionRegistry.CreateAttackAndStrength(damage, strength),
        ]);
    }
}
