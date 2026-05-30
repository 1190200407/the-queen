using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

/// <summary>每层使玩家回合结束时聚合体多执行一轮灯槽意图（在基础 1 次之上再加 <see cref="PowerModel.Amount"/> 次）。</summary>
public sealed class TerminusFormPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this)
        {
            return Task.CompletedTask;
        }

        CombatState? combatState = base.CombatState;
        Player? player = base.Owner.Player;
        if (combatState == null || player == null)
        {
            return Task.CompletedTask;
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, player);
        if (amalgam?.Monster is FriendlyAmalgam friendly)
        {
            friendly.RefreshDisplayedIntent();
        }

        return Task.CompletedTask;
    }
}
