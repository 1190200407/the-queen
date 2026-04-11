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
			// FromHandForUpgrade 的 source 非空时，手牌 UI 要等 source.ExecutionFinished 才把手牌从升级预览移回。
			// 本能力在同一钩子内连续多次选牌时，能力尚未结束，预览区里的 Card 会被下一次选择覆盖，导致只还原最后一次。
			CardModel? picked = await CardSelectCmd.FromHandForUpgrade(choiceContext, player, null!);
			if (picked == null)
			{
				break;
			}

			Flash();
			CardCmd.Upgrade(picked);
		}
	}
}
