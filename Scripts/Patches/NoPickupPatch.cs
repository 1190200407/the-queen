using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace ComicChess.TheQueen;


[HarmonyPatch]
internal static class ReleasePickupPatch
{
    /// <summary>
    /// Hook添加一段逻辑，让它会判断到卡牌本身的ShouldAddToDeck逻辑
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Hook), nameof(Hook.ShouldAddToDeck))]
    private static void ShouldAddToDeck_Postfix(ref bool __result, IRunState runState, CardModel card, ref AbstractModel? preventer)
    {
        if (card is Release)
        {
            __result = false;
            preventer = card;
        }
    }
}

[HarmonyPatch(typeof(MerchantCardEntry), "OnTryPurchase", new Type[] { typeof(MerchantInventory), typeof(bool) })]
internal static class ReleaseMerchantPurchasePatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Task<(bool, int)> __result, MerchantCardEntry __instance, bool ignoreCost)
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

[HarmonyPatch]
internal static class WrigglePickupPatch
{
    /// <summary>
    /// 参考放生：Hook 添加逻辑，让拾取时能走到卡牌本身的 ShouldAddToDeck / AfterAddToDeckPrevented。
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Hook), nameof(Hook.ShouldAddToDeck))]
    private static void ShouldAddToDeck_Postfix(ref bool __result, IRunState runState, CardModel card, ref AbstractModel? preventer)
    {
        _ = runState;
        if (card is Wriggle)
        {
            __result = false;
            preventer = card;
        }
    }
}

[HarmonyPatch(typeof(MerchantCardEntry), "OnTryPurchase", new Type[] { typeof(MerchantInventory), typeof(bool) })]
internal static class WriggleMerchantPurchasePatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Task<(bool, int)> __result, MerchantCardEntry __instance, bool ignoreCost)
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

[HarmonyPatch]
internal static class PaperCutsPickupPatch
{
    /// <summary>
    /// 参考放生/寄生：Hook 添加逻辑，让拾取时能走到卡牌本身的 ShouldAddToDeck / AfterAddToDeckPrevented。
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Hook), nameof(Hook.ShouldAddToDeck))]
    private static void ShouldAddToDeck_Postfix(ref bool __result, IRunState runState, CardModel card, ref AbstractModel? preventer)
    {
        _ = runState;
        if (card is PaperCuts)
        {
            __result = false;
            preventer = card;
        }
    }
}

[HarmonyPatch(typeof(MerchantCardEntry), "OnTryPurchase", new Type[] { typeof(MerchantInventory), typeof(bool) })]
internal static class PaperCutsMerchantPurchasePatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Task<(bool, int)> __result, MerchantCardEntry __instance, bool ignoreCost)
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

