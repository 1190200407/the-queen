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

/// <summary>蜷身：召唤并使聚合体获得若干层蜷身（<c>AmalgamCurlUpPower</c>，每场战斗首次受击生效）。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class CurlUp : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(7m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new PowerVar<AmalgamCurlUpPower>(8m),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<AmalgamCurlUpPower>(),
    ];

    public override int MaxUpgradeLevel => 0;

    public CurlUp()
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
            decimal amount = base.DynamicVars.Power<AmalgamCurlUpPower>().BaseValue;
            if (amount > 0m)
            {
                await PowerCmd.Apply<AmalgamCurlUpPower>(amalgam, amount, base.Owner.Creature, this);
            }
        }
    }
}
