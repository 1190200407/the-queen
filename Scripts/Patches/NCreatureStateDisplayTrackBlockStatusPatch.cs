using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class NCreatureStateDisplayTrackBlockStatusPatch : IPatchMethod
{
	private const int MaxDuplicateSubscriptions = 32;

	public static string PatchId => "thequeen_creature_state_display_block_track";
	public static string Description => "Clear duplicate BlockChanged subscriptions before re-track";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NCreatureStateDisplay), nameof(NCreatureStateDisplay.TrackBlockStatus)),
	];

	public static void Prefix(NCreatureStateDisplay __instance, Creature creature)
	{
		_ = creature;
		Creature? prev = Traverse.Create(__instance).Field<Creature?>("_blockTrackingCreature").Value;
		if (prev == null)
		{
			return;
		}

		var mi = AccessTools.DeclaredMethod(typeof(NCreatureStateDisplay), "OnBlockTrackingCreatureBlockChanged");
		if (mi == null)
		{
			return;
		}

		var handler = (Action<int, int>)Delegate.CreateDelegate(typeof(Action<int, int>), __instance, mi);
		for (int i = 0; i < MaxDuplicateSubscriptions; i++)
		{
			prev.BlockChanged -= handler;
		}
	}
}
