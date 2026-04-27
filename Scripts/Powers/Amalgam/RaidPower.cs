using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

public sealed class RaidPower : QueenPowerModel
{
    public bool GeneratesUpgradedTackle;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _ = choiceContext;
        if (player != base.Owner.Player || base.CombatState == null || base.Amount <= 0m)
        {
            return;
        }

        Flash();
        int count = (int)base.Amount;
        for (int i = 0; i < count; i++)
        {
            await QueenCardCmd.CreateInHand<Tackle>(player, base.CombatState, isUpgraded: GeneratesUpgradedTackle);
        }
    }
}
