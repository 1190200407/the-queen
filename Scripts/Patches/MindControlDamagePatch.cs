using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 精神控制：在 <see cref="CreatureCmd.Damage(PlayerChoiceContext, IEnumerable{Creature}, decimal, ValueProp, Creature, CardModel)"/> 入口改写 <c>targets</c>，
/// 使格挡与伤害整体落到新目标上；并用 <see cref="Hook.BeforeAttack"/> / <see cref="Hook.AfterAttack"/> 限定为敌方怪物正在执行的 <see cref="AttackCommand"/>。
/// </summary>
[HarmonyPatch]
internal static class MindControlDamagePatch
{
	private static int _monsterAttackCommandDepth;

	internal static bool IsInsideMonsterAttackCommand => _monsterAttackCommandDepth > 0;

	private static bool IsTrackedMonsterAttack(AttackCommand command)
	{
		if (command.Attacker is not { Side: CombatSide.Enemy, Monster: not null })
		{
			return false;
		}

		if (command.ModelSource is CardModel)
		{
			return false;
		}

		return command.DamageProps.HasFlag(ValueProp.Move) && !command.DamageProps.HasFlag(ValueProp.Unpowered);
	}

	[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeAttack))]
	[HarmonyPriority(Priority.First)]
	[HarmonyPrefix]
	private static void BeforeAttackPrefix(CombatState combatState, AttackCommand command)
	{
		_ = combatState;
		if (IsTrackedMonsterAttack(command))
		{
			_monsterAttackCommandDepth++;
		}
	}

	[HarmonyPatch(typeof(Hook), nameof(Hook.AfterAttack))]
	[HarmonyPriority(Priority.Last)]
	[HarmonyPostfix]
	private static void AfterAttackPostfix(CombatState combatState, AttackCommand command)
	{
		_ = combatState;
		if (IsTrackedMonsterAttack(command) && _monsterAttackCommandDepth > 0)
		{
			_monsterAttackCommandDepth--;
		}
	}

	[HarmonyPrefix]
	[HarmonyPriority(Priority.First)]
	[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new[]
	{
		typeof(PlayerChoiceContext),
		typeof(IEnumerable<Creature>),
		typeof(decimal),
		typeof(ValueProp),
		typeof(Creature),
		typeof(CardModel),
	})]
	private static void DamagePrefix(
		PlayerChoiceContext choiceContext,
		ref IEnumerable<Creature> targets,
		decimal amount,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource)
	{
		_ = choiceContext;
		_ = amount;
		if (targets is null || dealer is null)
		{
			return;
		}

		List<Creature> list = targets.ToList();
		if (list.Count == 0)
		{
			return;
		}

		CombatState? combatState = dealer.CombatState ?? list[0].CombatState;
		if (combatState is null)
		{
			return;
		}

		if (!MindControlPower.TryApplyRedirectToTargets(list, combatState, dealer, props, cardSource))
		{
			return;
		}

		targets = list;
	}
}
