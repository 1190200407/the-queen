using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 原版附魔图标只解析 <c>res://images/enchantments/</c>；暂用魂灯 buff 小图标代替。
/// </summary>
[HarmonyPatch(typeof(EnchantmentModel), nameof(EnchantmentModel.IconPath), MethodType.Getter)]
internal static class SoulLightEnchantmentIconPatch
{
	private const string SoulLampPackedIconPath = "res://TheQueen/images/powers/soul_lamp.png";

	[HarmonyPrefix]
	private static bool Prefix(EnchantmentModel __instance, ref string __result)
	{
		if (__instance is not SoulLight)
		{
			return true;
		}

		__result = ResourceLoader.Exists(SoulLampPackedIconPath)
			? SoulLampPackedIconPath
			: EnchantmentModel.MissingIconPath;
		return false;
	}
}
