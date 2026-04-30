using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>刺击：召唤聚合体、造成伤害，并偷取金币（进入 <see cref="HeistPower"/> 资金池）。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class GremlinStab : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new GoldVar(5),
    ];

    public GremlinStab()
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
        if (target.IsAlive)
        {
            decimal damage = base.DynamicVars.Damage.BaseValue;
            AmalgamActionModel? attack = AmalgamActionRegistry.CreateOffense(damage, target);
            if (attack != null)
            {
                await attack.ExecuteAsync(choiceContext, amalgam);
            }
        }

        decimal gold = base.DynamicVars.Gold.BaseValue;
        if (gold > 0m)
        {
            HeistPower? heist = amalgam.GetPower<HeistPower>();
            Creature? applier = amalgam.PetOwner?.Creature;
            if (heist is null)
            {
                await PowerCmd.Apply<HeistPower>(amalgam, gold, applier, this);
            }
            else
            {
                await PowerCmd.ModifyAmount(heist, gold, applier, this);
            }
        }
    }
}

