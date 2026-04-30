using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

/// <summary>聚合体从沉睡中苏醒时，按层数触发一次召唤。</summary>
public sealed class DormantRebirthPower : QueenPowerModel, IAmalgamEventListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnAmalgamWakeFromSleepAsync(CombatState combatState, Creature amalgam)
    {
        if (base.Owner.Player is not Player queen)
        {
            return;
        }
        if (amalgam.PetOwner != queen)
        {
            return;
        }

        Flash();
        await FriendlyAmalgamCmd.Summon(new ThrowingPlayerChoiceContext(), queen, Amount, this);
    }
}
