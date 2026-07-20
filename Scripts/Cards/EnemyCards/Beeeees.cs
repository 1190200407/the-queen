using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(EnemyCardPool))]
public sealed class Beeeees : LearnIntentCardModel
{
    private const decimal summon = 7m;
    private const decimal personalHiveStacks = 1m;
    private const decimal learnIntentDamagePerHit = 3m;
    private const int learnIntentHitCount = 7;
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(summon).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new PowerVar<AmalgamPersonalHivePower>(personalHiveStacks),
        new AmalgamLearnIntentDamageVar(learnIntentDamagePerHit, ValueProp.Move),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<AmalgamPersonalHivePower>(),
    ];

    public override int MaxUpgradeLevel => 0;

    public Beeeees()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);

        if (base.Owner.Creature.CombatState is { } combatState)
        {
            Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
            if (amalgam is { IsAlive: true })
            {
                decimal stacks = base.DynamicVars.Power<AmalgamPersonalHivePower>().BaseValue;
                await PowerCmd.Apply<AmalgamPersonalHivePower>(choiceContext, amalgam, stacks, base.Owner.Creature, this);
            }
        }

        await PlayLearnIntentsFromCreateAsync(choiceContext, cardPlay);
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal damagePerHit = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateOffenseMulti(damagePerHit, learnIntentHitCount);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}
