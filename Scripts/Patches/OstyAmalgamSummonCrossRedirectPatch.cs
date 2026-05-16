using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class OstyAmalgamSummonCrossRedirectPatch : IPatchMethod
{
	public static string PatchId => "thequeen_osty_summon_to_amalgam";
	public static string Description => "Redirect OstyCmd.Summon to amalgam when amalgam exists";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(OstyCmd), nameof(OstyCmd.Summon)),
	];

	[HarmonyPriority(Priority.First)]
	public static bool Prefix(
		PlayerChoiceContext choiceContext,
		Player summoner,
		decimal amount,
		AbstractModel? source,
		ref Task<SummonResult> __result)
	{
		if (summoner.Creature.CombatState is not { } combatState)
		{
			return true;
		}

		if (FriendlyAmalgamCmd.GetExisting(combatState, summoner) == null)
		{
			return true;
		}

		__result = SummonAmalgamInsteadAsync(choiceContext, summoner, amount, source);
		return false;
	}

	private static async Task<SummonResult> SummonAmalgamInsteadAsync(
		PlayerChoiceContext choiceContext,
		Player summoner,
		decimal amount,
		AbstractModel? source)
	{
		await FriendlyAmalgamCmd.Summon(choiceContext, summoner, amount, source);
		ICombatState? cs = summoner.Creature.CombatState;
		Creature? amalgam = cs == null ? null : FriendlyAmalgamCmd.GetExisting(cs, summoner);
		return new SummonResult(amalgam, amount);
	}
}
