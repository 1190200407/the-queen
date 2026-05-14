using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 原版 <see cref="AttackIntent.GetSingleDamage"/> 对怪物意图始终用本地玩家作 <c>ModifyDamage</c> 的 target；精神控制下实际可能打在其它敌方身上，
/// 此处对「除自身外的可攻击敌方」逐个跑 <see cref="Hook.ModifyDamage"/>，取最大整型伤害作为意图数字（与多敌预览常见做法一致）。
/// </summary>
[HarmonyPatch(typeof(AttackIntent), nameof(AttackIntent.GetSingleDamage))]
internal static class MindControlAttackIntentGetSingleDamagePatch
{
	[HarmonyPostfix]
	private static void GetSingleDamage_Postfix(AttackIntent __instance, IEnumerable<Creature> targets, Creature owner, ref int __result)
	{
		_ = targets;
		if (owner.CombatState is not CombatState combatState || !owner.IsEnemy)
		{
			return;
		}

		Player? me = LocalContext.GetMe(combatState);
		if (me == null)
		{
			return;
		}

		if (!owner.GetPowerInstances<MindControlPower>().Any(p => p.Applier == me.Creature))
		{
			return;
		}

		Func<decimal>? calc = __instance.DamageCalc;
		if (calc == null)
		{
			return;
		}

		decimal baseDamage = calc();
		List<Creature> candidates = combatState.HittableEnemies.Where(e => e.IsAlive && e != owner).ToList();
		if (candidates.Count == 0)
		{
			candidates.Add(owner);
		}

		decimal max = 0m;
		foreach (Creature target in candidates)
		{
			decimal n = Hook.ModifyDamage(
				me.RunState,
				combatState,
				target,
				owner,
				baseDamage,
				ValueProp.Move,
				null,
				ModifyDamageHookType.All,
				CardPreviewMode.None,
				out _);
			if (n > max)
			{
				max = n;
			}
		}

		__result = System.Math.Max(0, (int)max);
	}
}
