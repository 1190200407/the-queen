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
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>硬化外壳：使聚合体获得若干层原版硬化外壳。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class HardenedShell : QueenCardModel
{
    // 怪物牌不可升级：2(1) 直接落地为 1。
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(5m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new PowerVar<AmalgamHardenedShellPower>(20m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<AmalgamHardenedShellPower>(),
    ];

    public override int MaxUpgradeLevel => 0;

    public HardenedShell()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
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
            decimal amount = base.DynamicVars.Power<AmalgamHardenedShellPower>().BaseValue;
            if (amount > 0m)
            {
                await PowerCmd.Apply<AmalgamHardenedShellPower>(choiceContext, amalgam, amount, base.Owner.Creature, this);
            }
        }
    }
}

