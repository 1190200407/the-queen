using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 「消逝」：本应进入弃牌堆时改为进入消耗堆，并走消耗的历史与 AfterCardExhausted（不记为弃牌）。
/// </summary>
internal static class FadeOnDiscardPatchHelpers
{
	internal static bool HasFade(CardModel card) => card.HasModKeyword(QueenKeyword.Fade);
}

internal sealed class FadeOnDiscardCardPileCmdAddPatch : IPatchMethod
{
	public static string PatchId => "thequeen_fade_card_pile_cmd_add";
	public static string Description => "Fade: redirect single-card discard to exhaust pile";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CardPileCmd), nameof(CardPileCmd.Add),
		[
			typeof(CardModel),
			typeof(CardPile),
			typeof(CardPilePosition),
			typeof(AbstractModel),
			typeof(bool),
		]),
	];

	public static void Prefix(CardModel card, ref CardPile newPile)
	{
		if (newPile.Type != PileType.Discard || !FadeOnDiscardPatchHelpers.HasFade(card))
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
}

internal sealed class FadeOnDiscardCardPileAddInternalPatch : IPatchMethod
{
	public static string PatchId => "thequeen_fade_card_pile_add_internal";
	public static string Description => "Fade: batch discard redirect and exhaust notify";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CardPile), nameof(CardPile.AddInternal)),
	];

	public static bool Prefix(CardPile __instance, CardModel card, int index, bool silent)
	{
		if (__instance.Type != PileType.Discard || !FadeOnDiscardPatchHelpers.HasFade(card))
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

	public static void Postfix(CardPile __instance, CardModel card)
	{
		if (__instance.Type != PileType.Exhaust || !FadeOnDiscardTracker.ConsumePendingExhaustNotify(card))
		{
			return;
		}

		ICombatState? combatState = card.CombatState ?? card.Owner?.Creature.CombatState;
		if (combatState == null || CombatManager.Instance == null)
		{
			return;
		}

		FadeOnDiscardTracker.DeferExhaustNotify(combatState, card);
	}
}

/// <summary>
/// 在 <see cref="CardPileCmd.Add"/> 返回前同步触发消逝牌的 AfterCardExhausted，避免联机校验和分歧。
/// </summary>
internal sealed class FadeOnDiscardCardPileCmdAddFlushPatch : IPatchMethod
{
	public static string PatchId => "thequeen_fade_card_pile_cmd_add_flush";
	public static string Description => "Fade: flush deferred AfterCardExhausted before Add returns";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CardPileCmd), nameof(CardPileCmd.Add),
		[
			typeof(IEnumerable<CardModel>),
			typeof(CardPile),
			typeof(CardPilePosition),
			typeof(AbstractModel),
			typeof(bool),
			typeof(bool),
		]),
	];

	public static void Postfix(ref Task<IReadOnlyList<CardPileAddResult>> __result) =>
		__result = FlushAfterAddAsync(__result);

	private static async Task<IReadOnlyList<CardPileAddResult>> FlushAfterAddAsync(
		Task<IReadOnlyList<CardPileAddResult>> originalTask)
	{
		IReadOnlyList<CardPileAddResult> result = await originalTask;
		await FadeOnDiscardTracker.FlushPendingExhaustNotificationsAsync();
		return result;
	}
}

internal sealed class FadeOnDiscardCombatHistoryCardDiscardedPatch : IPatchMethod
{
	public static string PatchId => "thequeen_fade_combat_history_card_discarded";
	public static string Description => "Fade: skip discard history for fade cards";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CombatHistory), nameof(CombatHistory.CardDiscarded)),
	];

	public static bool Prefix(ICombatState combatState, CardModel card) =>
		!FadeOnDiscardPatchHelpers.HasFade(card);
}

internal sealed class FadeOnDiscardHookAfterCardDiscardedPatch : IPatchMethod
{
	public static string PatchId => "thequeen_fade_hook_after_card_discarded";
	public static string Description => "Fade: skip AfterCardDiscarded for fade cards";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(Hook), nameof(Hook.AfterCardDiscarded)),
	];

	public static bool Prefix(ICombatState combatState, PlayerChoiceContext choiceContext, CardModel card) =>
		!FadeOnDiscardPatchHelpers.HasFade(card);
}
