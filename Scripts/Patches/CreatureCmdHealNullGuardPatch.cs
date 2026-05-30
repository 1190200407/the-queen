using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 原版 <see cref="CreatureCmd.Heal"/> 在 <paramref name="creature"/> 为 null 时仍会访问 <c>creature.IsDead</c> 导致卡死。
/// 奥斯提召唤被聚合体顶替后，如 <c>Spur</c> 仍对 <c>Owner.Osty</c> 治疗时会传入 null。
/// </summary>
internal sealed class CreatureCmdHealNullGuardPatch : IPatchMethod
{
	public static string PatchId => "thequeen_creature_cmd_heal_null_guard";
	public static string Description => "Skip CreatureCmd.Heal when target creature is null";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CreatureCmd), nameof(CreatureCmd.Heal), new[]
		{
			typeof(Creature),
			typeof(decimal),
			typeof(bool),
		}),
	];

	public static bool Prefix(Creature creature, ref Task __result)
	{
		if (creature != null)
		{
			return true;
		}

		__result = Task.CompletedTask;
		return false;
	}
}
