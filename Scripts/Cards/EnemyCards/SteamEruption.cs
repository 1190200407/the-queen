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

/// <summary>蒸汽喷发：召唤并使聚合体获得蒸汽喷发（行动时叠加）。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class SteamEruption : QueenCardModel
{
    // 怪物牌不可升级：3 固定；召唤 10(14) 落地为 14。
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(14m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new PowerVar<AmalgamSteamEruptionPower>(15m),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<AmalgamSteamEruptionPower>(),
    ];

    public override int MaxUpgradeLevel => 0;

    public SteamEruption()
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
            decimal amount = base.DynamicVars.Power<AmalgamSteamEruptionPower>().BaseValue;
            if (amount > 0m)
            {
                await PowerCmd.Apply<AmalgamSteamEruptionPower>(amalgam, amount, base.Owner.Creature, this);
            }
        }
    }
}

