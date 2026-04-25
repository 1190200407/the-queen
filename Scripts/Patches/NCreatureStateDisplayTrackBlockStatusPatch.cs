using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace ComicChess.TheQueen;

/// <summary>
/// 原版 <see cref="NCreatureStateDisplay.TrackBlockStatus"/> 每次调用都会对 <c>BlockChanged</c> 再 <c>+=</c> 一次，
/// 而 <c>_ExitTree</c> 只 <c>-=</c> 一次；若同一血条节点被多次 Track（例如聚合体战斗壳 + 召唤、或叠在存活聚合体上反复 <c>TryTrackOwnerBlockOnAmalgamNode</c>），
/// 会在主人格挡变化时仍调用已销毁的 <see cref="NHealthBar"/>，触发 <c>ObjectDisposedException</c>。
/// 在重新订阅前对该处理器多次 <c>-=</c>（多余的 <c>-=</c> 为 no-op），清掉本节点在旧追踪生物上的全部重复订阅。
/// </summary>
[HarmonyPatch(typeof(NCreatureStateDisplay), nameof(NCreatureStateDisplay.TrackBlockStatus))]
internal static class NCreatureStateDisplayTrackBlockStatusPatch
{
	private const int MaxDuplicateSubscriptions = 32;

	[HarmonyPrefix]
	private static void Prefix(NCreatureStateDisplay __instance, Creature creature)
	{
		_ = creature;
		Creature? prev = Traverse.Create(__instance).Field<Creature?>("_blockTrackingCreature").Value;
		if (prev == null)
		{
			return;
		}

		var mi = AccessTools.DeclaredMethod(typeof(NCreatureStateDisplay), "OnBlockTrackingCreatureBlockChanged");
		if (mi == null)
		{
			return;
		}

		var handler = (Action<int, int>)Delegate.CreateDelegate(typeof(Action<int, int>), __instance, mi);
		for (int i = 0; i < MaxDuplicateSubscriptions; i++)
		{
			prev.BlockChanged -= handler;
		}
	}
}
