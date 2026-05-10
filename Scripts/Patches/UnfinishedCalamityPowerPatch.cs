using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>在敌人身上的负面能力被移除后，若仍存在 <see cref="UnfinishedCalamityPower"/> 标记，则触发其效果（移除标记本身不触发）。</summary>
[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Remove), new Type[] { typeof(PowerModel) })]
internal static class UnfinishedCalamityPowerPatch
{
	[HarmonyPostfix]
	private static async Task Postfix(Task __result, PowerModel? power)
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

		if (power.Type != PowerType.Debuff)
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
