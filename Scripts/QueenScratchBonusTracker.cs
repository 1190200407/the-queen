using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;

namespace ComicChess.TheQueen;

/// <summary>
/// 本场战斗中由 <see cref="Scratch"/> / <see cref="EndlessScratch"/> 累积的全局加成，
/// 在 <see cref="ScratchTaggedCard.AfterCardEnteredCombat"/> 中补全后入场的 <see cref="ScratchTaggedCard"/> 数值（含手牌生成、抽堆置入等）。
/// </summary>
internal static class QueenScratchBonusTracker
{
	private sealed class Entry
	{
		public int EndlessScratchLayers;
		public decimal ScratchDamageFromPlays;
	}

	private static readonly Dictionary<ulong, Entry> ByPlayer = new();

	private static Entry GetOrCreate(ulong netId)
	{
		if (!ByPlayer.TryGetValue(netId, out Entry? e))
		{
			e = new Entry();
			ByPlayer[netId] = e;
		}

		return e;
	}

	public static void RecordScratchPlay(Player owner, decimal increase)
	{
		if (owner.Character is not QueenCharacter)
		{
			return;
		}

		GetOrCreate(owner.NetId).ScratchDamageFromPlays += increase;
	}

	public static void RecordEndlessScratchPlay(Player owner)
	{
		if (owner.Character is not QueenCharacter)
		{
			return;
		}

		GetOrCreate(owner.NetId).EndlessScratchLayers++;
	}

	public static void ApplyToNewScratchTagged(Player owner, ScratchTaggedCard card)
	{
		if (owner.Character is not QueenCharacter || !ByPlayer.TryGetValue(owner.NetId, out Entry? e))
		{
			return;
		}

		if (e.ScratchDamageFromPlays > 0m)
		{
			card.BuffFromScratchPlay(e.ScratchDamageFromPlays);
		}

		for (int i = 0; i < e.EndlessScratchLayers; i++)
		{
			card.BuffHitCountFromEndlessScratch(1);
		}
	}

	public static void Reset() => ByPlayer.Clear();
}
