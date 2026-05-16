using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 门扉外壳：受到致命伤害时，消耗层数抵消“溢出伤害”，使生命值最多降到 1。
/// </summary>
public sealed class AmalgamDoormakerBossPower : QueenPowerModel
{
    private sealed class Data
    {
        public decimal PendingConsume;
    }

    protected override object? InitInternalData() => new Data();

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        _ = props;
        _ = dealer;
        _ = cardSource;

        if (target != base.Owner || amount <= 0m || Amount <= 0m || !base.Owner.IsAlive)
        {
            return amount;
        }

        Data data = GetInternalData<Data>();
        data.PendingConsume = 0m;

        decimal currentHp = base.Owner.CurrentHp;
        
        // 若本次扣血会致命：允许把生命压到 1，抵消剩余“溢出伤害”。
        // 需要抵消的溢出伤害 = amount - (currentHp - 1)
        decimal overflow = amount - (currentHp - 1m);
        if (overflow <= 0m)
        {
            return amount;
        }

        decimal consume = decimal.Min(overflow, Amount);
        if (consume <= 0m)
        {
            return amount;
        }

        data.PendingConsume = consume;
        return amount - consume;
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        Data data = GetInternalData<Data>();
        decimal consume = data.PendingConsume;
        if (consume <= 0m)
        {
            return;
        }

        data.PendingConsume = 0m;
        Flash();

        decimal next = Amount - consume;
        if (next <= 0m)
        {
            await PowerCmd.Remove(this);
            return;
        }

        await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, -consume, base.Owner, null);
    }
}

