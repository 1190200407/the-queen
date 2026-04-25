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
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>滑溜：使聚合体获得 1 层原版滑溜。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class Slippery : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SlipperyPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(1m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
    ];
    public override int MaxUpgradeLevel => 0;

    public Slippery()
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

        decimal stacks = base.DynamicVars["Summon"].BaseValue;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, stacks, this);

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is { IsAlive: true } && stacks > 0m)
        {
            await PowerCmd.Apply<SlipperyPower>(amalgam, stacks, base.Owner.Creature, this);
        }
    }
}
