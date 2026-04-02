using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
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
	private static bool HasFade(CardModel card) => card.Keywords.Contains(QueenKeyword.fade);

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

		CombatManager.Instance.History.CardExhausted(combatState, card);
		TaskHelper.RunSafely(Hook.AfterCardExhausted(combatState, new BlockingPlayerChoiceContext(), card, causedByEthereal: false));
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CombatHistory), nameof(CombatHistory.CardDiscarded))]
	private static bool CardDiscarded_Prefix(CombatState combatState, CardModel card) => !HasFade(card);

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardDiscarded))]
	private static bool AfterCardDiscarded_Prefix(CombatState combatState, PlayerChoiceContext choiceContext, CardModel card) => !HasFade(card);
}

