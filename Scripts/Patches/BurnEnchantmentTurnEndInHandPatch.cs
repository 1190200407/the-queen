using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

[HarmonyPatch(typeof(CardModel), "get_HasTurnEndInHandEffect")]
internal static class BurnEnchantmentTurnEndInHandPatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, ref bool __result)
    {
        if (__result)
        {
            return;
        }

        // Treat Burn-enchanted cards as having an end-of-turn-in-hand effect
        // so UI/engine can flag them similarly to status cards like Burn.
        if (!__instance.IsEnchantmentPreview && __instance.Enchantment is Burn)
        {
            __result = true;
        }
    }
}

