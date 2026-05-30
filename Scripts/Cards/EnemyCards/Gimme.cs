using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>给我！：召唤并使聚合体获得偷窃。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class Gimme : QueenCardModel
{
    // 怪物牌不可升级：召唤3(5) 直接落地为 5。
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(5m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new PowerVar<AmalgamThieveryPower>(5m),
        new PowerVar<AmalgamEscapePower>(3m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<AmalgamThieveryPower>(),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override int MaxUpgradeLevel => 0;

    public Gimme()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (base.Owner.Creature.CombatState is not { } combatState)
        {
            return;
        }

        decimal summon = base.DynamicVars.Summon.BaseValue;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, summon, this);

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is { IsAlive: true })
        {
            decimal stacks = base.DynamicVars.Power<AmalgamThieveryPower>().BaseValue;
            if (stacks > 0m)
            {
                await PowerCmd.Apply<AmalgamThieveryPower>(choiceContext, amalgam, stacks, base.Owner.Creature, this);
            }
            decimal escape = base.DynamicVars.Power<AmalgamEscapePower>().BaseValue;
            if (escape > 0m)
            {
                await PowerCmd.Apply<AmalgamEscapePower>(choiceContext, amalgam, escape, base.Owner.Creature, this);
            }
        }
    }
}

