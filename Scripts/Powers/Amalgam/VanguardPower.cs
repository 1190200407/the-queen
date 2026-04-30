using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Combat;

namespace ComicChess.TheQueen;

public sealed class VanguardPower : QueenPowerModel, IAmalgamEventListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnAmalgamActAsync(CombatState combatState, PlayerChoiceContext choiceContext, Creature amalgam)
    {
        if (amalgam.PetOwner is not Player queen)
        {
            return;
        }
        if (amalgam != base.Owner || !amalgam.IsAlive)
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
