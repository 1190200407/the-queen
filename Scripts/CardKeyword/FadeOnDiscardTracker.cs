using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 追踪「本应进弃牌堆、经 Patch 改入消耗堆」的牌，用于补发 <see cref="MegaCrit.Sts2.Core.Combat.History.CombatHistory.CardExhausted"/> 与 <see cref="MegaCrit.Sts2.Core.Hooks.Hook.AfterCardExhausted"/>。
/// </summary>
internal static class FadeOnDiscardTracker
{
	private static readonly HashSet<CardModel> _pendingExhaustNotify = new();

	internal static void MarkPendingExhaustNotify(CardModel card) => _pendingExhaustNotify.Add(card);

	internal static bool ConsumePendingExhaustNotify(CardModel card) => _pendingExhaustNotify.Remove(card);
}
