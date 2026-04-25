using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

public sealed class VanguardPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    internal async Task OnAmalgamActAsync(PlayerChoiceContext choiceContext, Player queen)
    {
        if (queen.Creature != base.Owner || !queen.Creature.IsAlive)
        {
            return;
        }

        int draw = (int)Amount;
        if (draw <= 0)
        {
            return;
        }

        Flash();
        await CardPileCmd.Draw(choiceContext, draw, queen);
    }
}
