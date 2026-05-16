using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>坠击：聚合体对随机敌人造成多段伤害�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class DropStrike : QueenCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    private const int hitCount = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new AmalgamLearnIntentDamageVar(5m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("AttackAll").WithMultiplier((CardModel card, Creature? _) => 
        {
            CombatState? combatState = card.Owner.Creature.CombatState;
            if (combatState is null)
            {
                return 1m;
            }
            Player queen = card.Owner;
            if (AmalgamOffenseTargeting.ResolveMode(combatState, queen) == AmalgamOffenseTargetingMode.AllAliveEnemies)
            {
                return 1m;
            }
            return 0m;
        })
    ];

    /// <summary>无友方聚合体�?<see cref="FriendlyAmalgam.BlocksDirectOffenseFromHand"/> 时手牌红高亮（直接由聚合体结算多段伤害）�?/summary>
    protected override bool ShouldGlowRedInternal =>
        (base.Owner?.Creature?.CombatState is { } combatState
            && (FriendlyAmalgamCmd.GetExisting(combatState, base.Owner) is not { Monster: FriendlyAmalgam amalgam }
                || amalgam.BlocksDirectOffenseFromHand))
        || base.ShouldGlowRedInternal;

    public DropStrike()
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

        var amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is not { Monster: FriendlyAmalgam amalgamModel })
        {
            return;
        }

        if (amalgamModel.BlocksDirectOffenseFromHand)
        {
            return;
        }

        decimal dmg = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        await FriendlyAmalgamCmd.ExecuteMultiHitOffense(choiceContext, amalgam, dmg, hitCount);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["LearnIntentDamage"].UpgradeValueBy(1m);
    }
}

