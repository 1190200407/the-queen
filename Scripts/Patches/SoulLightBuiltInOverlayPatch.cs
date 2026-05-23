using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 非 <see cref="QueenCardModel"/> 的牌附魔 <see cref="SoulLight"/> 时，与 <see cref="QueenCardModel.HasSelfBound"/> 一样走内置叠层预览。
/// </summary>
internal sealed class SoulLightBuiltInOverlayPatch : IPatchMethod
{
	public static string PatchId => "thequeen_soullight_builtin_overlay";
	public static string Description => "Soul Light enchantment advertises built-in bound overlay preview";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CardModel), nameof(CardModel.HasBuiltInOverlay), MethodType.Getter),
	];

	public static void Postfix(CardModel __instance, ref bool __result)
	{
		if (__result || __instance is QueenCardModel)
		{
			return;
		}

		if (__instance.Enchantment is SoulLight &&
			(__instance.CombatState == null || __instance.Affliction is Bound))
		{
			__result = true;
		}
	}
}
