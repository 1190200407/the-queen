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

/// <summary>饥饿（噬尸蛞蝓）：召唤、使聚合体获得饥饿，并学习进攻意图。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class CorpseSlugHunger : LearnIntentCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(10m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new IntVar("RavenousStacks", 4m),
        new AmalgamLearnIntentDamageVar(6m, ValueProp.Move),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<AmalgamRavenousPower>(),
    ];

    public override int MaxUpgradeLevel => 0;

    protected override bool ShouldSummonBeforeLearnIntent => true;

    public CorpseSlugHunger()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task AfterSummonBeforeLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (base.Owner.Creature.CombatState is not { } combatState)
        {
            return;
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is not { IsAlive: true })
        {
            return;
        }

        decimal stacks = base.DynamicVars["RavenousStacks"].BaseValue;
        if (stacks > 0m)
        {
            await PowerCmd.Apply<AmalgamRavenousPower>(amalgam, stacks, base.Owner.Creature, this);
        }
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (base.Owner.Creature.CombatState is not { } combatState)
        {
            return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([]);
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is not { IsAlive: true })
        {
            return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([]);
        }

        decimal dmg = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateOffense(dmg);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}
