using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
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

/// <summary>自爆：聚合体失去所有当前生命，对所有敌人造成等量伤害。聚合体已死亡时仍可打出但无效果。消耗（升级后不再消耗）。</summary>
[RegisterCard(typeof(QueenCardPool))]
public sealed class SelfDestruct : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Unpowered).WithMultiplier(static (card, _) => GetAmalgamCurrentHp(card)),
    ];

    protected override bool ShouldGlowRedInternal =>
        base.Owner?.Creature?.CombatState is not { } combatState
        || base.Owner == null
        || !HasAmalgam(combatState, base.Owner);

    protected override bool IsPlayable
    {
        get
        {
            CombatState? combatState = base.Owner?.Creature?.CombatState;
            if (combatState == null || base.Owner == null)
            {
                return base.IsPlayable;
            }

            return HasAmalgam(combatState, base.Owner);
        }
    }

    public SelfDestruct()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (base.Owner.Creature.CombatState is not CombatState combatState)
        {
            return;
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam?.Monster is not FriendlyAmalgam)
        {
            return;
        }

        if (amalgam.CurrentHp <= 0m)
        {
            return;
        }

        await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Unpowered()
            .WithNoAttackerAnim()
            .TargetingAllOpponents(combatState)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);

        await CreatureCmd.Kill(amalgam);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }

    private static decimal GetAmalgamCurrentHp(CardModel card)
    {
        CombatState? combatState = card.Owner?.Creature?.CombatState;
        if (combatState == null || card.Owner == null)
        {
            return 0m;
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, card.Owner);
        return amalgam?.Monster is FriendlyAmalgam ? amalgam.CurrentHp : 0m;
    }

    private static bool HasAmalgam(CombatState combatState, Player owner)
    {
        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, owner);
        return amalgam?.Monster is FriendlyAmalgam;
    }
}
