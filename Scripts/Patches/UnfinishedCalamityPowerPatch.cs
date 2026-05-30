using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class UnfinishedCalamityPowerPatch : IPatchMethod
{
	public static string PatchId => "thequeen_unfinished_calamity_power_remove";
	public static string Description => "Trigger Unfinished Calamity after enemy debuff removed";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(PowerCmd), nameof(PowerCmd.Remove), new[] { typeof(PowerModel) }),
	];

	public static async Task Postfix(Task __result, PowerModel? power)
	{
		await __result;
		if (power == null)
		{
			return;
		}

		if (CombatManager.Instance.IsEnding)
		{
			return;
		}

		if (!QueenDebuffUtil.IsDebuff(power))
		{
			return;
		}

		Creature? victim = power.Owner;
		if (victim is not { IsAlive: true, IsEnemy: true })
		{
			return;
		}

		CombatState? combatState = victim.CombatState;
		if (combatState == null)
		{
			return;
		}

		await UnfinishedCalamityPower.TryApplyAfterEnemyDebuffRemoved(combatState, victim, power);
	}
}
