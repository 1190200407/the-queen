using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal static class MindControlDamagePatchState
{
	private static int _monsterAttackCommandDepth;

	private static readonly Stack<MindControlAttackFrame> AttackFrames = new();

	internal static bool IsInsideMonsterAttackCommand => _monsterAttackCommandDepth > 0;

	internal static MindControlAttackFrame? TryPeekAttackFrame() => AttackFrames.Count > 0 ? AttackFrames.Peek() : null;

	internal static bool IsTrackedMonsterAttack(AttackCommand command)
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

	internal static void EnterMonsterAttack(AttackCommand command)
	{
		if (!IsTrackedMonsterAttack(command))
		{
			return;
		}

		_monsterAttackCommandDepth++;
		AttackFrames.Push(new MindControlAttackFrame());
	}

	internal static void ExitMonsterAttack(AttackCommand command)
	{
		if (!IsTrackedMonsterAttack(command) || _monsterAttackCommandDepth <= 0)
		{
			return;
		}

		_monsterAttackCommandDepth--;
		if (AttackFrames.Count > 0)
		{
			MindControlAttackFrame frame = AttackFrames.Pop();
			frame.FlushDecrements();
		}
	}
}

internal sealed class MindControlHookBeforeAttackPatch : IPatchMethod
{
	public static string PatchId => "thequeen_mind_control_before_attack";
	public static string Description => "Mind control: track monster attack command";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(Hook), nameof(Hook.BeforeAttack)),
	];

	[HarmonyPriority(Priority.First)]
	public static void Prefix(ICombatState combatState, AttackCommand command)
	{
		_ = combatState;
		MindControlDamagePatchState.EnterMonsterAttack(command);
	}
}

internal sealed class MindControlHookAfterAttackPatch : IPatchMethod
{
	public static string PatchId => "thequeen_mind_control_after_attack";
	public static string Description => "Mind control: flush attack frame";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(Hook), nameof(Hook.AfterAttack)),
	];

	[HarmonyPriority(Priority.Last)]
	public static void Postfix(ICombatState combatState, AttackCommand command)
	{
		_ = combatState;
		MindControlDamagePatchState.ExitMonsterAttack(command);
	}
}

internal sealed class MindControlCreatureCmdDamagePatch : IPatchMethod
{
	public static string PatchId => "thequeen_mind_control_creature_cmd_damage";
	public static string Description => "Mind control: redirect damage targets";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new[]
		{
			typeof(PlayerChoiceContext),
			typeof(IEnumerable<Creature>),
			typeof(decimal),
			typeof(ValueProp),
			typeof(Creature),
			typeof(CardModel),
			typeof(CardPlay),
		}),
	];

	[HarmonyPriority(Priority.First)]
	public static void Prefix(
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

		ICombatState? combatState = dealer.CombatState ?? list[0].CombatState;
		if (combatState is null)
		{
			return;
		}

		if (!MindControlPower.TryApplyRedirectToTargets(
			    list,
			    combatState,
			    dealer,
			    props,
			    cardSource,
			    MindControlDamagePatchState.TryPeekAttackFrame()))
		{
			return;
		}

		targets = list;
	}
}
