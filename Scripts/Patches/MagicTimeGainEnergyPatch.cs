using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class MagicTimeGainEnergyPatch : IPatchMethod
{
	public static string PatchId => "thequeen_magic_time_gain_energy";
	public static string Description => "Magic Time: auto-refill soul lamp after gaining energy";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(PlayerCmd), nameof(PlayerCmd.GainEnergy)),
	];

	public static async Task Postfix(Task __result, decimal amount, Player player)
	{
		await __result;
		if (amount <= 0m || player?.Creature is null)
		{
			return;
		}

		MagicTimePower? magicTime = player.Creature.GetPower<MagicTimePower>();
		if (magicTime == null)
		{
			return;
		}

		await magicTime.TryRefillIfNeeded(new ThrowingPlayerChoiceContext(), player);
	}
}
