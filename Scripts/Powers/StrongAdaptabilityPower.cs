using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

public sealed class StrongAdaptabilityPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != base.Owner.Player)
		{
			return;
		}

		int n = (int)base.Amount;
		for (int i = 0; i < n; i++)
		{
			CardModel? picked = await CardSelectCmd.FromHandForUpgrade(choiceContext, player, this);
			if (picked == null)
			{
				break;
			}

			Flash();
			CardCmd.Upgrade(picked);
		}
	}
}
