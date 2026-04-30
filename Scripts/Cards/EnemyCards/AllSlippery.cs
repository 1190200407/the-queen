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

/// <summary>非常滑溜：使聚合体获得多层原版滑溜。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class AllSlippery : QueenCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<AmalgamSlipperyPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(10m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new IntVar("SlipperyStacks", 4m),
    ];
    public override int MaxUpgradeLevel => 0;

    public AllSlippery()
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

        decimal summonStacks = base.DynamicVars["Summon"].BaseValue;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, summonStacks, this);

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is { IsAlive: true } && summonStacks > 0m)
        {
            decimal slipperyStacks = base.DynamicVars["SlipperyStacks"].BaseValue;
            await PowerCmd.Apply<AmalgamSlipperyPower>(amalgam, slipperyStacks, base.Owner.Creature, this);
        }
    }
}
