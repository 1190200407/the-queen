using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class QueenSummonOnCombatStartPatch : IPatchMethod
{
	public static string PatchId => "thequeen_summon_on_combat_start";
	public static string Description => "Spawn amalgam combat shell for queen at combat start";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(Hook), nameof(Hook.BeforeCombatStart)),
	];

	public static async Task Postfix(Task __result, IRunState runState, CombatState? combatState)
	{
		_ = runState;
		await __result;

		if (combatState == null)
		{
			return;
		}

		var choiceContext = new BlockingPlayerChoiceContext();
		foreach (Player player in combatState.Players)
		{
			if (player.Character is not QueenCharacter)
			{
				continue;
			}

			await FriendlyAmalgamCmd.EnsureAmalgamCombatStartShellAsync(choiceContext, player);
		}
	}
}
