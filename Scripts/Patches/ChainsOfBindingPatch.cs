using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class ChainsOfBindingHookBeforeCombatStartPatch : IPatchMethod
{
	public static string PatchId => "thequeen_chains_before_combat_start";
	public static string Description => "Chains of binding: reset combat tracker";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(Hook), nameof(Hook.BeforeCombatStart)),
	];

	public static async Task Postfix(Task __result, IRunState runState, CombatState? combatState)
	{
		_ = runState;
		_ = combatState;
		await __result;
		ChainsOfBindingBoundTracker.ResetCombat();
	}
}

internal sealed class ChainsOfBindingPowerShouldPlayPatch : IPatchMethod
{
	public static string PatchId => "thequeen_chains_should_play";
	public static string Description => "Chains of binding: queen bypasses play lock";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(ChainsOfBindingPower), nameof(ChainsOfBindingPower.ShouldPlay)),
	];

	public static void Postfix(CardModel card, ref bool __result)
	{
		_ = card;
		__result = true;
	}
}

internal sealed class ChainsOfBindingPowerBeforeCardPlayedPatch : IPatchMethod
{
	public static string PatchId => "thequeen_chains_before_card_played";
	public static string Description => "Chains of binding: skip vanilla bound tracking for queen";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(ChainsOfBindingPower), nameof(ChainsOfBindingPower.BeforeCardPlayed)),
	];

	public static bool Prefix(ChainsOfBindingPower __instance, CardPlay cardPlay, ref Task __result)
	{
		_ = __instance;
		_ = cardPlay;
		__result = Task.CompletedTask;
		return false;
	}
}

internal sealed class ChainsOfBindingPowerAfterCardDrawnPatch : IPatchMethod
{
	public static string PatchId => "thequeen_chains_after_card_drawn";
	public static string Description => "Chains of binding: custom bound-on-draw logic";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(ChainsOfBindingPower), nameof(ChainsOfBindingPower.AfterCardDrawn)),
	];

	public static bool Prefix(
		ChainsOfBindingPower __instance,
		PlayerChoiceContext choiceContext,
		CardModel card,
		bool fromHandDraw,
		ref Task __result)
	{
		_ = choiceContext;
		_ = fromHandDraw;
		__result = AfterCardDrawn_ChainsAsync(__instance, card);
		return false;
	}

	private static async Task AfterCardDrawn_ChainsAsync(ChainsOfBindingPower power, CardModel card)
	{
		Player? player = power.Owner?.Player;
		if (card.Owner != player || power.CombatState.CurrentSide != power.Owner.Side)
		{
			return;
		}

		ChainsOfBindingBoundTracker.RegisterCardDrawn(player);
		if (!ModelDb.Affliction<Bound>().CanAfflict(card))
		{
			return;
		}

		int cap = power.Amount;
		if (!ChainsOfBindingBoundTracker.CanApplyAnotherCard(player, cap))
		{
			return;
		}

		await CardCmd.AfflictAndPreview<Bound>(
			new List<CardModel> { card },
			power.Amount,
			CardPreviewStyle.None);
		ChainsOfBindingBoundTracker.RegisterChainsBoundCard(player, card);
	}
}

internal sealed class ChainsOfBindingPowerBeforeTurnEndPatch : IPatchMethod
{
	public static string PatchId => "thequeen_chains_before_turn_end";
	public static string Description => "Chains of binding: clear chains-bound cards end of turn";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(ChainsOfBindingPower), nameof(ChainsOfBindingPower.BeforeSideTurnEnd)),
	];

	public static bool Prefix(
		ChainsOfBindingPower __instance,
		PlayerChoiceContext choiceContext,
		CombatSide side,
		ref Task __result)
	{
		_ = choiceContext;

		object? internalData = Traverse.Create(__instance).Field("_internalData").GetValue();
		if (internalData != null)
		{
			Traverse.Create(internalData).Field("boundCardPlayed").SetValue(false);
		}

		if (side == __instance.Owner.Side)
		{
			ChainsOfBindingBoundTracker.ClearChainsBoundsEndOfTurn(__instance.Owner.Player);
		}

		__result = Task.CompletedTask;
		return false;
	}
}
