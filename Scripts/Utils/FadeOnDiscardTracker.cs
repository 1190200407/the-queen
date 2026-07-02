using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 追踪「本应进弃牌堆、经 Patch 改入消耗堆」的牌，用于补发
/// <see cref="CombatHistory.CardExhausted"/> 与 <see cref="Hook.AfterCardExhausted"/>。
/// 通知必须在 <see cref="MegaCrit.Sts2.Core.Commands.CardPileCmd.Add"/> 完成前同步 flush，以保证联机校验和一致。
/// </summary>
internal static class FadeOnDiscardTracker
{
	private static readonly HashSet<CardModel> _pendingExhaustNotify = new();
<<<<<<< HEAD
	private static readonly List<(CombatState CombatState, CardModel Card)> _deferredNotifications = new();
=======
	private static readonly List<(ICombatState CombatState, CardModel Card)> _deferredNotifications = new();
>>>>>>> beta

	internal static void MarkPendingExhaustNotify(CardModel card) => _pendingExhaustNotify.Add(card);

	internal static bool ConsumePendingExhaustNotify(CardModel card) => _pendingExhaustNotify.Remove(card);

<<<<<<< HEAD
	internal static void DeferExhaustNotify(CombatState combatState, CardModel card) =>
=======
	internal static void DeferExhaustNotify(ICombatState combatState, CardModel card) =>
>>>>>>> beta
		_deferredNotifications.Add((combatState, card));

	internal static async Task FlushPendingExhaustNotificationsAsync()
	{
		if (_deferredNotifications.Count == 0)
		{
			return;
		}

<<<<<<< HEAD
		(CombatState CombatState, CardModel Card)[] pending = _deferredNotifications.ToArray();
		_deferredNotifications.Clear();

		foreach ((CombatState combatState, CardModel card) in pending)
=======
		(ICombatState CombatState, CardModel Card)[] pending = _deferredNotifications.ToArray();
		_deferredNotifications.Clear();

		foreach ((ICombatState combatState, CardModel card) in pending)
>>>>>>> beta
		{
			if (CombatManager.Instance == null || CombatManager.Instance.IsOverOrEnding)
			{
				continue;
			}

			CombatManager.Instance.History.CardExhausted(combatState, card);
			await Hook.AfterCardExhausted(combatState, new BlockingPlayerChoiceContext(), card, causedByEthereal: false);
		}
	}
}
