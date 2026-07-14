using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class ChainsOfBindingCardClonePatch : IPatchMethod
{
	public static string PatchId => "thequeen_chains_card_clone_tracking";
	public static string Description => "Chains of binding: track clones of cards bound by chains";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CardModel), nameof(CardModel.CreateClone)),
	];

	public static void Postfix(CardModel __instance, CardModel __result)
	{
		ChainsOfBindingBoundTracker.RegisterCloneIfSourceTracked(__instance, __result);
	}
}
