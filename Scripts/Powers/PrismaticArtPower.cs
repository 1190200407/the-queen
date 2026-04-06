using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace ComicChess.TheQueen;

/// <summary>
/// 战斗胜利结算前追加额外选卡奖励；卡池合并方式与原版 <c>PrismaticGem</c> 一致。
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
			options = ApplyPrismaticGemPools(player, options);
			room.AddExtraReward(player, new CardReward(options, 3, player));
		}

		return Task.CompletedTask;
	}

	/// <summary>对齐 <see cref="MegaCrit.Sts2.Core.Models.Relics.PrismaticGem.ModifyCardRewardCreationOptions"/>。</summary>
	private static CardCreationOptions ApplyPrismaticGemPools(Player player, CardCreationOptions options)
	{
		if (options.Flags.HasFlag(CardCreationFlags.NoCardPoolModifications))
		{
			return options;
		}

		if (options.CustomCardPool != null)
		{
			return options;
		}

		if (options.CardPools.All(static (CardPoolModel p) => p.IsColorless))
		{
			return options;
		}

		IEnumerable<CardPoolModel> pools = player.UnlockState.CharacterCardPools.Union(options.CardPools);
		return options.WithCardPools(pools, options.CardPoolFilter);
	}
}
