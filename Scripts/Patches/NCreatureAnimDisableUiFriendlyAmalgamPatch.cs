using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class NCreatureAnimDisableUiFriendlyAmalgamPatch : IPatchMethod
{
	public static string PatchId => "thequeen_amalgam_anim_disable_ui";
	public static string Description => "Friendly amalgam skips UI fade on AnimDisableUi";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NCreature), nameof(NCreature.AnimDisableUi)),
	];

	public static bool Prefix(NCreature __instance, ref Tween __result)
	{
		if (__instance.Entity?.Monster is FriendlyAmalgam)
		{
			Tween tween = __instance.CreateTween();
			tween.TweenInterval(0.001);
			__result = tween;
			return false;
		}

		return true;
	}
}
