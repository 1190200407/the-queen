using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class NCardRewardSelectionScreenSelectCardGuardPatch : IPatchMethod
{
	public static string PatchId => "thequeen_card_reward_select_guard";
	public static string Description => "Ignore duplicate SelectCard after reward TCS completed";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NCardRewardSelectionScreen), "SelectCard", new[] { typeof(NCardHolder) }),
	];

	public static bool Prefix(NCardRewardSelectionScreen __instance)
	{
		FieldInfo? field = AccessTools.Field(typeof(NCardRewardSelectionScreen), "_completionSource");
		if (field?.GetValue(__instance) is not TaskCompletionSource<Tuple<IEnumerable<NCardHolder>, bool>> tcs)
		{
			return true;
		}

		return !tcs.Task.IsCompleted;
	}
}
