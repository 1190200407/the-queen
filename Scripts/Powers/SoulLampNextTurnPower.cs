using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

public sealed class SoulLampNextTurnPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterEnergyReset(Player player)
	{
		if (player != base.Owner.Player)
		{
			return;
		}

		if (base.Amount > 0)
		{
			await QueenCardCmd.AddSoulLamp(new ThrowingPlayerChoiceContext(), player, base.Amount);
		}

		await PowerCmd.Remove(this);
	}
}
