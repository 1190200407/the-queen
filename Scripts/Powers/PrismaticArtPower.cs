using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace ComicChess.TheQueen;

/// <summary>
/// 战斗胜利结算前追加额外选卡奖励；卡池对齐原版 <c>Kaleidoscope</c>，仅含其他角色颜色（不含女王）。
/// </summary>
public sealed class PrismaticArtPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override Task AfterCombatEnd(CombatRoom room)
	{
		Player? player = base.Owner.Player;
		if (player == null)
		{
			return Task.CompletedTask;
		}

		for (int i = 0; i < base.Amount; i++)
		{
			CardCreationOptions options = CardCreationOptions.ForRoom(player, room.RoomType);
			options = QueenCrossColorCardSource.WithOtherCharacterPoolsOnly(player, options);
			room.AddExtraReward(player, new CardReward(options, 3, player));
		}

		return Task.CompletedTask;
	}

}
