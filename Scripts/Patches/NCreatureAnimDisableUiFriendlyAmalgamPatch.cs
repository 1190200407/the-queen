using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace ComicChess.TheQueen;

/// <summary>
/// 原版 <see cref="NCreature.AnimDie"/> 开头无条件调用 <see cref="NCreature.AnimDisableUi"/>，会把血条淡出；
/// 友方聚合体被击倒后仍留在场上（击退沉睡），与 <see cref="FriendlyAmalgam.IsHealthBarVisible"/>「始终显示血条」不一致。
/// 对聚合体节点跳过淡出，改为立即结束的 tween，避免 <see cref="NCreature.AnimDie"/> 等待无效动画。
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.AnimDisableUi))]
internal static class NCreatureAnimDisableUiFriendlyAmalgamPatch
{
	[HarmonyPrefix]
	private static bool Prefix(NCreature __instance, ref Tween __result)
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
