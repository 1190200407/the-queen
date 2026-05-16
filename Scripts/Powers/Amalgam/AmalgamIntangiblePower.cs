using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>无实体（聚合体版）：将聚合体受到的所有伤害与生命减少效果降低为 1（持续按层数回合）。</summary>
public sealed class AmalgamIntangiblePower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/intangible_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/intangible_power.png";

    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        _ = props;
        _ = dealer;
        _ = cardSource;

        if (target != base.Owner || !base.Owner.IsAlive)
        {
            return amount;
        }

        if (amount <= 0m)
        {
            return amount;
        }

        return Math.Min(1m, amount);
    }

    public override Task AfterModifyingHpLostAfterOsty()
    {
        Flash();
        return Task.CompletedTask;
    }
}

