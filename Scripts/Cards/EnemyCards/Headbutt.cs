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
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

/// <summary>头槌：召唤；聚合体获得失衡；通过 <see cref="FriendlyAmalgamCmd.CombineIntent"/> 学习聚合进攻意图。消耗。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class Headbutt : LearnIntentCardModel
{
    /// <summary>与 <see cref="AmalgamCompositeIntentAction"/> 及盛碗虫（石）捕获映射共用。</summary>
    public const string BowlbugRockCompositeKey = "BOWLBUG";

    private const decimal summon = 3m;
    private const decimal learnIntentDamage = 15m;
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<string> RegisteredKeywordIds => [QueenKeyword.AmalgamComposite];

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(summon).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentDamageVar(learnIntentDamage, ValueProp.Move),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        QueenHoverTips.LearnIntent,
        ModKeywordRegistry.CreateHoverTip(QueenKeyword.AmalgamComposite),
        HoverTipFactory.FromPower<AmalgamImbalancedPower>(),
    ];

    public Headbutt()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);

        if (base.Owner.Creature.CombatState is not { } combatState)
        {
            return;
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is not { IsAlive: true })
        {
            return;
        }

        if (amalgam.GetPower<AmalgamImbalancedPower>() == null)
        {
            await PowerCmd.Apply<AmalgamImbalancedPower>(amalgam, 1m, base.Owner.Creature, this);
        }

        decimal damage = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        await FriendlyAmalgamCmd.CombineIntent(
            choiceContext,
            base.Owner,
            new AmalgamOffenseIntentAction(damage),
            this,
            BowlbugRockCompositeKey);
    }
}
