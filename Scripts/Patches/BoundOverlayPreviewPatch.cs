using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;

namespace ComicChess.TheQueen;

/// <summary>
/// <see cref="QueenCardModel.HasBuiltInOverlay"/> 依赖 <see cref="CardModel.CreateOverlay"/>，
/// 但后者非 virtual，无法加载原版魂缚叠层；用 Prefix 替换为 afflictions/bound 场景。
/// 战斗内若已无 <see cref="Bound"/>（如澄净之源清除），走原版逻辑，避免叠层与数据不一致。
/// </summary>
[HarmonyPatch]
internal static class BoundOverlayPreviewPatch
{
	private const string BoundAfflictionOverlayInnerPath = "cards/overlays/afflictions/bound";

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CardModel), nameof(CardModel.CreateOverlay))]
	private static bool CreateOverlay_Prefix(CardModel __instance, ref Control __result)
	{
		if (__instance is QueenCardModel queen && queen.HasSelfBound)
		{
			if (__instance.CombatState != null && __instance.Affliction is not Bound)
			{
				return true;
			}

			__result = PreloadManager.Cache.GetScene(SceneHelper.GetScenePath(BoundAfflictionOverlayInnerPath))
				.Instantiate<Control>(PackedScene.GenEditState.Disabled);
			return false;
		}

		return true;
	}
}
