using System;
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

/// <summary>尖叫：使聚合体获得“尖叫”层数（=当前生命值一半）。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class Shriek : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new SummonVar(20m).WithTooltip("QUEEN_SUMMON_DYNAMIC")];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<AmalgamShriekPower>(),
    ];

    public override int MaxUpgradeLevel => 0;

    public Shriek()
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

        decimal stacks = base.DynamicVars.Summon.BaseValue;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, stacks, this);

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is not { IsAlive: true })
        {
            return;
        }

        decimal threshold = Math.Floor(amalgam.CurrentHp / 2m);
        if (threshold <= 0m)
        {
            return;
        }

        await PowerCmd.Apply<AmalgamShriekPower>(amalgam, threshold, applier: base.Owner.Creature, cardSource: this);
    }
}

