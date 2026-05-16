using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class BurnEnchantmentTurnEndInHandPatch : IPatchMethod
{
	public static string PatchId => "thequeen_burn_turn_end_in_hand";
	public static string Description => "Burn enchantment: flag HasTurnEndInHandEffect";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CardModel), nameof(CardModel.HasTurnEndInHandEffect), MethodType.Getter),
	];

	public static void Postfix(CardModel __instance, ref bool __result)
	{
		if (__result)
		{
			return;
		}

		if (!__instance.IsEnchantmentPreview && __instance.Enchantment is Burn)
		{
			__result = true;
		}
	}
}
