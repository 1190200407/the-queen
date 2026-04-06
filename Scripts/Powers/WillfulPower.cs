using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

public sealed class WillfulPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != base.Owner.Player)
		{
			return;
		}

		SoulLampPower? lamp = base.Owner.GetPower<SoulLampPower>();
		int current = lamp != null ? lamp.DisplayAmount : 0;
		if (current > (int)base.Amount)
		{
			return;
		}

		await QueenCardCmd.AddSoulLamp(base.Owner.Player, 1);
	}
}
