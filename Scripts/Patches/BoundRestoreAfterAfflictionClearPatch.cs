// using HarmonyLib;
// using MegaCrit.Sts2.Core.Commands;
// using MegaCrit.Sts2.Core.Helpers;
// using MegaCrit.Sts2.Core.Models;
// using MegaCrit.Sts2.Core.Models.Afflictions;

// namespace ComicChess.TheQueen;

// /// <summary>
// /// 对自带魂缚展示层（HasSelfBound=true）的女王牌，
// /// 在 ClearAfflictionInternal 后若处于无侵蚀状态，则恢复 Bound。
// /// </summary>
// [HarmonyPatch(typeof(CardModel), "ClearAfflictionInternal")]
// internal static class BoundRestoreAfterAfflictionClearPatch
// {
// 	/// <summary>大于 0 时不自动补回魂缚（例如澄净之源等有意永久清除侵蚀）。支持嵌套。</summary>
// 	internal static int SuppressReapplyDepth { get; private set; }

// 	internal static void EnterSuppressReapply() => SuppressReapplyDepth++;

// 	internal static void ExitSuppressReapply() => SuppressReapplyDepth--;

// 	[HarmonyPostfix]
// 	private static void ClearAfflictionInternal_Postfix(CardModel __instance)
// 	{
// 		if (__instance == null || SuppressReapplyDepth > 0)
// 		{
// 			return;
// 		}
// 		if (__instance is not QueenCardModel queen || !queen.HasSelfBound || __instance.Affliction != null)
// 		{
// 			return;
// 		}

// 		TaskHelper.RunSafely(CardCmd.Afflict<Bound>(__instance, 1m));
// 	}
// }
