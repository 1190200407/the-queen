using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class ReleasePickupShouldAddToDeckPatch : IPatchMethod
{
	public static string PatchId => "thequeen_release_should_add_to_deck";
	public static string Description => "Release: block deck add, route to card logic";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(Hook), nameof(Hook.ShouldAddToDeck)),
	];

	public static void Postfix(ref bool __result, IRunState runState, CardModel card, ref AbstractModel? preventer)
	{
		_ = runState;
		if (card is Release)
		{
			__result = false;
			preventer = card;
		}
	}
}

internal sealed class ReleaseMerchantPurchasePatch : IPatchMethod
{
	public static string PatchId => "thequeen_release_merchant_purchase";
	public static string Description => "Release: allow merchant purchase without deck add";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(MerchantCardEntry), "OnTryPurchase", new[] { typeof(MerchantInventory), typeof(bool) }),
	];

	public static void Postfix(ref Task<(bool, int)> __result, MerchantCardEntry __instance, bool ignoreCost)
	{
		__result = AdjustMerchantPurchaseAsync(__result, __instance, ignoreCost);
	}

	private static async Task<(bool, int)> AdjustMerchantPurchaseAsync(
		Task<(bool, int)> original,
		MerchantCardEntry __instance,
		bool ignoreCost)
	{
		(bool success, int goldSpent) = await original.ConfigureAwait(false);
		if (success)
		{
			return (success, goldSpent);
		}

		if (__instance.CreationResult?.Card is Release)
		{
			return (true, ignoreCost ? 0 : __instance.Cost);
		}

		return (success, goldSpent);
	}
}

internal sealed class WrigglePickupShouldAddToDeckPatch : IPatchMethod
{
	public static string PatchId => "thequeen_wriggle_should_add_to_deck";
	public static string Description => "Wriggle: block deck add, route to card logic";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(Hook), nameof(Hook.ShouldAddToDeck)),
	];

	public static void Postfix(ref bool __result, IRunState runState, CardModel card, ref AbstractModel? preventer)
	{
		_ = runState;
		if (card is Wriggle)
		{
			__result = false;
			preventer = card;
		}
	}
}

internal sealed class WriggleMerchantPurchasePatch : IPatchMethod
{
	public static string PatchId => "thequeen_wriggle_merchant_purchase";
	public static string Description => "Wriggle: allow merchant purchase without deck add";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(MerchantCardEntry), "OnTryPurchase", new[] { typeof(MerchantInventory), typeof(bool) }),
	];

	public static void Postfix(ref Task<(bool, int)> __result, MerchantCardEntry __instance, bool ignoreCost)
	{
		__result = AdjustMerchantPurchaseAsync(__result, __instance, ignoreCost);
	}

	private static async Task<(bool, int)> AdjustMerchantPurchaseAsync(
		Task<(bool, int)> original,
		MerchantCardEntry __instance,
		bool ignoreCost)
	{
		(bool success, int goldSpent) = await original.ConfigureAwait(false);
		if (success)
		{
			return (success, goldSpent);
		}

		if (__instance.CreationResult?.Card is Wriggle)
		{
			return (true, ignoreCost ? 0 : __instance.Cost);
		}

		return (success, goldSpent);
	}
}

internal sealed class PaperCutsPickupShouldAddToDeckPatch : IPatchMethod
{
	public static string PatchId => "thequeen_paper_cuts_should_add_to_deck";
	public static string Description => "PaperCuts: block deck add, route to card logic";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(Hook), nameof(Hook.ShouldAddToDeck)),
	];

	public static void Postfix(ref bool __result, IRunState runState, CardModel card, ref AbstractModel? preventer)
	{
		_ = runState;
		if (card is PaperCuts)
		{
			__result = false;
			preventer = card;
		}
	}
}

internal sealed class PaperCutsMerchantPurchasePatch : IPatchMethod
{
	public static string PatchId => "thequeen_paper_cuts_merchant_purchase";
	public static string Description => "PaperCuts: allow merchant purchase without deck add";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(MerchantCardEntry), "OnTryPurchase", new[] { typeof(MerchantInventory), typeof(bool) }),
	];

	public static void Postfix(ref Task<(bool, int)> __result, MerchantCardEntry __instance, bool ignoreCost)
	{
		__result = AdjustMerchantPurchaseAsync(__result, __instance, ignoreCost);
	}

	private static async Task<(bool, int)> AdjustMerchantPurchaseAsync(
		Task<(bool, int)> original,
		MerchantCardEntry __instance,
		bool ignoreCost)
	{
		(bool success, int goldSpent) = await original.ConfigureAwait(false);
		if (success)
		{
			return (success, goldSpent);
		}

		if (__instance.CreationResult?.Card is PaperCuts)
		{
			return (true, ignoreCost ? 0 : __instance.Cost);
		}

		return (success, goldSpent);
	}
}
