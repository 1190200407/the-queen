using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

/// <summary>活力火花：使聚合体获得活力火花。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class VitalSpark : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    private const decimal vitalSparkStacks = 1m;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<AmalgamVitalSparkPower>(vitalSparkStacks),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        EnergyHoverTip
    ];

    public override int MaxUpgradeLevel => 0;

    public VitalSpark()
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

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is { IsAlive: true })
        {
            if (vitalSparkStacks > 0m)
            {
                await PowerCmd.Apply<AmalgamVitalSparkPower>(amalgam, vitalSparkStacks, base.Owner.Creature, this);
            }
        }
    }
}

