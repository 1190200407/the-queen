using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// <see cref="QueenCardModel.HasBuiltInOverlay"/> 依赖 <see cref="CardModel.CreateOverlay"/>，
/// 但后者非 virtual，无法加载原版魂缚叠层；用 Prefix 替换为 afflictions/bound 场景。
/// </summary>
[HarmonyPatch]
internal static class BoundOverlayPreviewPatch
{
	private const string BoundAfflictionOverlayInnerPath = "cards/overlays/afflictions/bound";

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CardModel), nameof(CardModel.CreateOverlay))]
	private static bool CreateOverlay_Prefix(CardModel __instance, ref Control __result)
	{
		if (__instance is QueenCardModel queen && queen.UseBoundAfflictionOverlayForPreview)
		{
			__result = PreloadManager.Cache.GetScene(SceneHelper.GetScenePath(BoundAfflictionOverlayInnerPath))
				.Instantiate<Control>(PackedScene.GenEditState.Disabled);
			return false;
		}

		return true;
	}
}
