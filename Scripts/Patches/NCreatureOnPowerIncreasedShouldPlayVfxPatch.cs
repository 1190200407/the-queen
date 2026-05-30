using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 原版 <see cref="NCreature"/> 在能力层数变化时，抖动只判断 Buff/Debuff，不 respect <see cref="PowerModel.ShouldPlayVfx"/>。
/// 女王魂灯等隐藏能力需完全跳过施加演出。
/// </summary>
internal sealed class NCreatureOnPowerIncreasedShouldPlayVfxPatch : IPatchMethod
{
	public static string PatchId => "thequeen_ncreature_power_increased_should_play_vfx";
	public static string Description => "Skip power apply VFX/shake when ShouldPlayVfx is false";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NCreature), "OnPowerIncreased"),
	];

	public static bool Prefix(PowerModel power, bool silent)
	{
		if (!silent && CombatManager.Instance.IsInProgress && !power.ShouldPlayVfx)
		{
			return false;
		}

		return true;
	}
}
