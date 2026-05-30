using MegaCrit.Sts2.Core.Nodes.RestSite;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class QueenRestSiteHideFlameGlowPatch : IPatchMethod
{
	public static string PatchId => "thequeen_rest_site_hide_flame_glow";
	public static string Description => "Queen rest site: hide shader flame glow when campfire goes out";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter.HideFlameGlow)),
	];

	public static bool Prefix(NRestSiteCharacter __instance)
	{
		if (__instance is NQueenRestSiteCharacter queen)
		{
			queen.HideQueenFlameGlow();
			return false;
		}

		return true;
	}
}
