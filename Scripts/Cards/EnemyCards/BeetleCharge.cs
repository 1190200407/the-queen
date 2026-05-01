using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>甲虫冲锋：仅当聚合体生命值不高于 50% 时可打出；聚合体造成重击；消耗。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class BeetleCharge : QueenCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    private const decimal damage = 30m;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new AmalgamLearnIntentDamageVar(damage, ValueProp.Move),
    ];

    public override int MaxUpgradeLevel => 0;

    protected override bool IsPlayable
    {
        get
        {
            CombatState? combatState = base.Owner?.Creature?.CombatState;
            if (combatState == null || base.Owner == null)
            {
                return true;
            }

            Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
            if (amalgam is not { IsAlive: true } || amalgam.MaxHp <= 0)
            {
                return false;
            }

            return amalgam.CurrentHp * 2 <= amalgam.MaxHp;
        }
    }

    public BeetleCharge()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (base.Owner.Creature.CombatState is not { } combatState)
        {
            return;
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is not { IsAlive: true })
        {
            return;
        }

        Creature target = cardPlay.Target;
        decimal dmg = base.DynamicVars["LearnIntentDamage"].BaseValue;
        if (target.IsAlive && dmg > 0m)
        {
            AmalgamActionModel? attack = AmalgamActionRegistry.CreateOffense(dmg, target);
            if (attack != null)
            {
                await attack.ExecuteAsync(choiceContext, amalgam);
            }
        }
    }
}

