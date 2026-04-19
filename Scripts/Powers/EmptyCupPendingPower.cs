using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

/// <summary>
/// <see cref="AmalgamEmptyCupIntentAction"/> 每执行一次为女王 <see cref="PowerCmd.Apply{T}"/> 叠 <strong>1</strong> 层；
/// 玩家回合开始时按当前层数各：+1 能量、抽 1，然后移除。
/// </summary>
public sealed class EmptyCupPendingPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != base.Owner.Player || !base.Owner.IsAlive)
        {
            return;
        }

        int n = (int)base.Amount;
        if (n <= 0)
        {
            await PowerCmd.Remove(this);
            return;
        }

        for (int i = 0; i < n; i++)
        {
            await PlayerCmd.GainEnergy(1m, player);
            await CardPileCmd.Draw(choiceContext, 1, player);
        }

        await PowerCmd.Remove(this);
    }
}
