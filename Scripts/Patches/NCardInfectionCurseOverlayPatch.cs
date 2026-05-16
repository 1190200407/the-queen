using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class NCardInfectionCurseOverlayPatch : IPatchMethod
{
	private static readonly string InfectionOverlayInnerPath = "cards/overlays/infection";

	private static readonly FieldInfo OverlayContainerField =
		AccessTools.Field(typeof(NCard), "_overlayContainer")!;

	private static readonly FieldInfo CardOverlayField =
		AccessTools.Field(typeof(NCard), "_cardOverlay")!;

	public static string PatchId => "thequeen_ncard_infection_overlay";
	public static string Description => "Infested/Lash cards use infection overlay scene";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NCard), "ReloadOverlay"),
	];

	public static void Postfix(NCard __instance)
	{
		CardModel? model = __instance.Model;
		if (model?.Enchantment is not Infested && model is not Lash)
		{
			return;
		}

		var overlayContainer = (Node)OverlayContainerField.GetValue(__instance)!;
		var oldOverlay = (Control?)CardOverlayField.GetValue(__instance);
		if (oldOverlay != null)
		{
			overlayContainer.RemoveChildSafely(oldOverlay);
			oldOverlay.QueueFreeSafely();
			CardOverlayField.SetValue(__instance, null);
		}

		string path = SceneHelper.GetScenePath(InfectionOverlayInnerPath);
		if (!ResourceLoader.Exists(path, string.Empty))
		{
			return;
		}

		Node root = PreloadManager.Cache.GetScene(path).Instantiate();
		if (root is not Control created)
		{
			root.QueueFree();
			return;
		}

		overlayContainer.AddChildSafely(created);
		CardOverlayField.SetValue(__instance, created);
	}
}
