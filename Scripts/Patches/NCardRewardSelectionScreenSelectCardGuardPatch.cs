using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

namespace ComicChess.TheQueen;

/// <summary>
/// 奖励选卡：首张选定后 <see cref="Release"/> 会在 <see cref="MegaCrit.Sts2.Core.Commands.CardPileCmd.Add"/> 内弹出二次选牌，
/// 子界面关闭时可能再次触发同一 <c>NCardHolder.Pressed</c>，对已完成的 <c>TaskCompletionSource</c> 再次 <c>SetResult</c> 会抛错。
/// 忽略在 TCS 已结束后的重复 <c>SelectCard</c>。
/// </summary>
[HarmonyPatch(typeof(NCardRewardSelectionScreen), "SelectCard", new Type[] { typeof(NCardHolder) })]
internal static class NCardRewardSelectionScreenSelectCardGuardPatch
{
	[HarmonyPrefix]
	private static bool Prefix(NCardRewardSelectionScreen __instance)
	{
		System.Reflection.FieldInfo? field = AccessTools.Field(typeof(NCardRewardSelectionScreen), "_completionSource");
		if (field?.GetValue(__instance) is not TaskCompletionSource<Tuple<IEnumerable<NCardHolder>, bool>> tcs)
		{
			return true;
		}

		if (tcs.Task.IsCompleted)
		{
			return false;
		}

		return true;
	}
}
