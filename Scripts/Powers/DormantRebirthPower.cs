using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

/// <summary>聚合体从沉睡中苏醒时，按层数触发一次召唤。</summary>
public sealed class DormantRebirthPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    internal async Task OnAmalgamWakeFromSleepAsync(Player queen)
    {
        if (queen.Creature != base.Owner || Amount <= 0m)
        {
            return;
        }

        Flash();
        await FriendlyAmalgamCmd.Summon(new ThrowingPlayerChoiceContext(), queen, Amount, this);
    }
}
