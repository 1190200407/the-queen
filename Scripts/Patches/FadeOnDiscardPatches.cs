using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 「消逝」：本应进入弃牌堆时改为进入消耗堆，并走消耗的历史与 AfterCardExhausted（不记为弃牌）。
/// 判定：<see cref="QueenCardModel.HasFadeOnDiscardKeyword"/>。
/// </summary>
[HarmonyPatch]
internal static class FadeOnDiscardPatches
{
	private static readonly object ExhaustNotifyQueueLock = new object();
	private static Task ExhaustNotifyQueue = Task.CompletedTask;

	private static bool HasFade(CardModel card) => card.Keywords.Contains(QueenKeyword.fade);

	private static void EnqueueFadeExhaustNotify(CombatState combatState, CardModel card)
	{
		lock (ExhaustNotifyQueueLock)
		{
			// 串行化消逝触发，避免多张牌同帧并发导致依赖计数的遗物（如 JozzPaper）重复结算。
			ExhaustNotifyQueue = ExhaustNotifyQueue.ContinueWith(
				_ => NotifyFadeExhausted(combatState, card),
				TaskScheduler.Default).Unwrap();
		}
	}

	private static async Task NotifyFadeExhausted(CombatState combatState, CardModel card)
	{
		CombatManager.Instance.History.CardExhausted(combatState, card);
		await Hook.AfterCardExhausted(combatState, new BlockingPlayerChoiceContext(), card, causedByEthereal: false);
	}

	/// <summary>
	/// 单卡 <see cref="CardPileCmd.Add(CardModel, CardPile, CardPilePosition, AbstractModel?, bool)"/>（含 <c>Add(card, PileType.Discard)</c>、<see cref="CardCmd.Discard"/> 的逐张弃牌）
	/// 在入口把目标堆改为消耗堆，使 <see cref="CardPileCmd"/> 内动画与 <see cref="CardModel.Pile"/> 解析一致。
	/// 批量 <c>Add(IEnumerable, discardPile)</c> 仍走下方 <see cref="CardPile.AddInternal"/> 前缀兜底。
	/// </summary>
	[HarmonyPrefix]
	[HarmonyPatch(
		typeof(CardPileCmd),
		nameof(CardPileCmd.Add),
		[
			typeof(CardModel),
			typeof(CardPile),
			typeof(CardPilePosition),
			typeof(AbstractModel),
			typeof(bool),
		])]
	private static void AddSingleToDiscard_RedirectFadeToExhaust(CardModel card, ref CardPile newPile)
	{
		if (newPile.Type != PileType.Discard || !HasFade(card))
		{
			return;
		}

		Player? owner = card.Owner;
		if (owner == null)
		{
			return;
		}

		CardPile? exhaust = CardPile.Get(PileType.Exhaust, owner);
		if (exhaust == null)
		{
			return;
		}

		FadeOnDiscardTracker.MarkPendingExhaustNotify(card);
		newPile = exhaust;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CardPile), nameof(CardPile.AddInternal))]
	private static bool AddInternal_Prefix(CardPile __instance, CardModel card, int index, bool silent)
	{
		if (__instance.Type != PileType.Discard || !HasFade(card))
		{
			return true;
		}

		Player? owner = card.Owner;
		if (owner == null)
		{
			return true;
		}

		CardPile? exhaust = CardPile.Get(PileType.Exhaust, owner);
		if (exhaust == null)
		{
			return true;
		}

		FadeOnDiscardTracker.MarkPendingExhaustNotify(card);
		exhaust.AddInternal(card, index, silent);
		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(CardPile), nameof(CardPile.AddInternal))]
	private static void AddInternal_Postfix(CardPile __instance, CardModel card)
	{
		if (__instance.Type != PileType.Exhaust || !FadeOnDiscardTracker.ConsumePendingExhaustNotify(card))
		{
			return;
		}

		CombatState? combatState = card.CombatState ?? card.Owner?.Creature.CombatState;
		if (combatState == null || CombatManager.Instance == null)
		{
			return;
		}

		EnqueueFadeExhaustNotify(combatState, card);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CombatHistory), nameof(CombatHistory.CardDiscarded))]
	private static bool CardDiscarded_Prefix(CombatState combatState, CardModel card) => !HasFade(card);

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardDiscarded))]
	private static bool AfterCardDiscarded_Prefix(CombatState combatState, PlayerChoiceContext choiceContext, CardModel card) => !HasFade(card);
}

