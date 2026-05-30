using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class MindControlAttackIntentGetSingleDamagePatch : IPatchMethod
{
	public static string PatchId => "thequeen_mind_control_attack_intent_damage";
	public static string Description => "Mind control: intent damage uses max ModifyDamage across enemies";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(AttackIntent), nameof(AttackIntent.GetSingleDamage)),
	];

	public static void Postfix(AttackIntent __instance, IEnumerable<Creature> targets, Creature owner, ref int __result)
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

		__result = Math.Max(0, (int)max);
	}
}
