using System.Reflection;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 附魔变化时刷新叠层，否则 <see cref="SoulLight"/> 刚加上时不会立刻出现魂缚预览。
/// </summary>
internal sealed class NCardSoulLightOverlayRefreshPatch : IPatchMethod
{
	private static readonly MethodInfo ReloadOverlayMethod =
		typeof(NCard).GetMethod("ReloadOverlay", BindingFlags.NonPublic | BindingFlags.Instance)
		?? throw new MissingMethodException(typeof(NCard).FullName, "ReloadOverlay");

	public static string PatchId => "thequeen_ncard_soullight_overlay_refresh";
	public static string Description => "Reload card overlay when enchantment changes";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NCard), "OnEnchantmentChanged"),
	];

	public static void Postfix(NCard __instance)
	{
		ReloadOverlayMethod.Invoke(__instance, null);
	}
}
