using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

public sealed class AmalgamSlipperyPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://images/atlases/power_atlas.sprites/slippery_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/slippery_power.png";

    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner)
            return amount;

        if (amount > 0)
            return 1;
        return amount;
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        Flash();
        await PowerCmd.Decrement(this);
    }
}