using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class BoundOverlayPreviewPatch : IPatchMethod
{
	private const string BoundAfflictionOverlayInnerPath = "vfx/ui/card/afflictions/bound/vfx_ui_card_affliction_bound";

	public static string PatchId => "thequeen_bound_overlay_preview";
	public static string Description => "Self-bound queen cards use bound affliction overlay";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CardModel), nameof(CardModel.CreateOverlay)),
	];

	public static bool Prefix(CardModel __instance, ref Control __result)
	{
		bool previewBound =
			(__instance is QueenCardModel queen && queen.HasSelfBound) ||
			__instance.Enchantment is SoulLight;

		if (!previewBound)
		{
			return true;
		}

		if (__instance.CombatState != null && __instance.Affliction is not Bound)
		{
			return true;
		}

		__result = PreloadManager.Cache.GetScene(SceneHelper.GetScenePath(BoundAfflictionOverlayInnerPath))
			.Instantiate<Control>(PackedScene.GenEditState.Disabled);
		return false;
	}
}
