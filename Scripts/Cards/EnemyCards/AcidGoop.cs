using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>酸液：召唤；聚合体获得力量；并学习进攻意图。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class AcidGoop : LearnIntentCardModel
{
    private const decimal learnIntentDamage = 4m;
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(3m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentDamageVar(learnIntentDamage, ValueProp.Move),
        new PowerVar<StrengthPower>(2m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    protected override bool ShouldSummonBeforeLearnIntent => true;
    public override int MaxUpgradeLevel => 0;

    public AcidGoop()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task AfterSummonBeforeLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (base.Owner.Creature.CombatState is not { } combatState)
        {
            return;
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is { IsAlive: true })
        {
            decimal strength = base.DynamicVars.Power<StrengthPower>().BaseValue;
            if (strength > 0m)
            {
                await PowerCmd.Apply<StrengthPower>(choiceContext, amalgam, strength, base.Owner.Creature, this);
            }
        }
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal dmg = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateOffense(dmg);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}
