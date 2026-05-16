using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal static class CaptureSingleTargetPreviewEntry
{
	internal static void TryBeginForCardPlay(NCardPlay play, TargetType targetType)
	{
		if (targetType != TargetType.AnyEnemy)
		{
			return;
		}

		if (play is NMouseCardPlay mousePlay)
		{
			CancellationTokenSource? cts = Traverse.Create(mousePlay)
				.Field<CancellationTokenSource>("_cancellationTokenSource")
				.Value;
			if (cts is not null && cts.IsCancellationRequested)
			{
				return;
			}
		}

		CardModel? card = Traverse.Create(play).Property("Card").GetValue<CardModel>();
		if (card is null || card is not ICanMonsterCapture cap)
		{
			return;
		}

		Player? owner = card.Owner;
		CombatState? combat = card.CombatState;
		if (owner is null || combat is null)
		{
			return;
		}

		IReadOnlyList<Creature> enemies = combat.GetOpponentsOf(owner.Creature).Where(static c => c.IsHittable).ToList();
		bool anyPreview = false;
		foreach (Creature e in enemies)
		{
			if (e.Monster is null || !cap.CanCapture(e.Monster, combat))
			{
				continue;
			}

			if (CaptureRewardPreviewRules.TryCreatePreviewCard(owner, e) is not null)
			{
				anyPreview = true;
				break;
			}
		}

		if (!anyPreview)
		{
			return;
		}

		EnemyIntentRewardCardPreview.ShowAllRewardCards(
			owner,
			enemies,
			(o, target) =>
			{
				if (target.Monster is null || !cap.CanCapture(target.Monster, combat))
				{
					return null;
				}

				return CaptureRewardPreviewRules.TryCreatePreviewCard(o, target);
			});
	}
}

internal sealed class EnemyIntentRewardCardPreview_NMouseCardPlay_SingleCreatureTargeting_Patch : IPatchMethod
{
	public static string PatchId => "thequeen_enemy_intent_preview_mouse_targeting";
	public static string Description => "Capture reward preview on mouse single-target play";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NMouseCardPlay), "SingleCreatureTargeting", new[] { typeof(TargetMode), typeof(TargetType) }),
	];

	public static void Prefix(NMouseCardPlay __instance, TargetMode targetMode, TargetType targetType)
	{
		_ = __instance;
		CaptureSingleTargetPreviewEntry.TryBeginForCardPlay(__instance, targetType);
	}
}

internal sealed class EnemyIntentRewardCardPreview_NControllerCardPlay_SingleCreatureTargeting_Patch : IPatchMethod
{
	public static string PatchId => "thequeen_enemy_intent_preview_controller_targeting";
	public static string Description => "Capture reward preview on controller single-target play";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NControllerCardPlay), "SingleCreatureTargeting", new[] { typeof(TargetType) }),
	];

	public static void Prefix(NControllerCardPlay __instance, TargetType targetType)
	{
		CaptureSingleTargetPreviewEntry.TryBeginForCardPlay(__instance, targetType);
	}
}

internal sealed class EnemyIntentRewardCardPreview_NCard_SetPreviewTarget_Patch : IPatchMethod
{
	public static string PatchId => "thequeen_enemy_intent_preview_set_preview_target";
	public static string Description => "Sync enlarged preview with hover target";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NCard), nameof(NCard.SetPreviewTarget)),
	];

	public static void Postfix(NCard __instance, Creature? creature)
	{
		CardModel? played = __instance.Model;
		if (played is null || played is not ICanMonsterCapture cap)
		{
			return;
		}

		if (creature is null || creature.Monster is null || played.CombatState is not CombatState combat
		    || !cap.CanCapture(creature.Monster, combat))
		{
			EnemyIntentRewardCardPreview.SyncEnlargeWithPreviewTarget(null);
			return;
		}

		EnemyIntentRewardCardPreview.SyncEnlargeWithPreviewTarget(creature);
	}
}

internal sealed class EnemyIntentRewardCardPreview_NCardPlay_Cleanup_Patch : IPatchMethod
{
	public static string PatchId => "thequeen_enemy_intent_preview_card_play_cleanup";
	public static string Description => "Hide capture previews when card play cleans up";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NCardPlay), "Cleanup"),
	];

	public static void Postfix()
	{
		EnemyIntentRewardCardPreview.HideAllRewardCards();
	}
}

internal sealed class EnemyIntentRewardCardPreview_Hook_AfterCombatEnd_Patch : IPatchMethod
{
	public static string PatchId => "thequeen_enemy_intent_preview_after_combat_end";
	public static string Description => "Reset capture preview state after combat";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(Hook), nameof(Hook.AfterCombatEnd)),
	];

	public static async Task Postfix(Task __result, IRunState runState, ICombatState? combatState, CombatRoom room)
	{
		_ = runState;
		_ = combatState;
		_ = room;
		await __result;
		EnemyIntentRewardCardPreview.ResetAfterCombat();
	}
}
