using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;

namespace ComicChess.TheQueen;

/// <summary>
/// 魔法时间：在获得能量后，若魂灯为 0 且有能量，则立刻执行“1 能量 -> 1 魂灯”的补灯逻辑。
/// </summary>
[HarmonyPatch(typeof(PlayerCmd), nameof(PlayerCmd.GainEnergy))]
internal static class MagicTimeGainEnergyPatch
{
	[HarmonyPostfix]
	private static void Postfix(decimal amount, Player player)
	{
		if (amount <= 0m || player?.Creature is null)
		{
			return;
		}

		Task task = MagicTimePower.TryAutoRefillSoulLamp(player);
		MegaCrit.Sts2.Core.Helpers.TaskHelper.RunSafely(task);
	}
}
