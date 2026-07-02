using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Potions;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal static class EcosphereBottleTargetingPreview
{
	private static bool _active;

	private static Callable? _creatureHoveredCallable;

	private static Callable? _creatureUnhoveredCallable;

	internal static void Begin(Player owner)
	{
		End();

		ICombatState? combat= owner.Creature.CombatState;
		if (combat == null)
		{
			return;
		}

		IReadOnlyList<Creature> enemies = combat.GetOpponentsOf(owner.Creature).Where(static c => c.IsHittable).ToList();
		bool anyPreview = false;
		foreach (Creature e in enemies)
		{
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

		NTargetManager tm = NTargetManager.Instance;
		_creatureHoveredCallable = Callable.From<NCreature>(OnCreatureHovered);
		_creatureUnhoveredCallable = Callable.From<NCreature>(OnCreatureUnhovered);
		tm.Connect(NTargetManager.SignalName.CreatureHovered, _creatureHoveredCallable.Value);
		tm.Connect(NTargetManager.SignalName.CreatureUnhovered, _creatureUnhoveredCallable.Value);

		EnemyIntentRewardCardPreview.ShowAllRewardCards(
			owner,
			enemies,
			CaptureRewardPreviewRules.TryCreatePreviewCard);
		_active = true;
	}

	internal static void End()
	{
		if (!_active)
		{
			return;
		}

		_active = false;
		EnemyIntentRewardCardPreview.SyncEnlargeWithPreviewTarget(null);
		EnemyIntentRewardCardPreview.HideAllRewardCards();

		NTargetManager tm = NTargetManager.Instance;
		if (_creatureHoveredCallable.HasValue)
		{
			tm.Disconnect(NTargetManager.SignalName.CreatureHovered, _creatureHoveredCallable.Value);
			_creatureHoveredCallable = null;
		}

		if (_creatureUnhoveredCallable.HasValue)
		{
			tm.Disconnect(NTargetManager.SignalName.CreatureUnhovered, _creatureUnhoveredCallable.Value);
			_creatureUnhoveredCallable = null;
		}
	}

	private static void OnCreatureHovered(NCreature creature)
	{
		EnemyIntentRewardCardPreview.SyncEnlargeWithPreviewTarget(creature.Entity);
	}

	private static void OnCreatureUnhovered(NCreature _)
	{
		EnemyIntentRewardCardPreview.SyncEnlargeWithPreviewTarget(null);
	}
}

internal sealed class EcosphereBottleTargetingPreview_NPotionHolder_TargetNode_Patch : IPatchMethod
{
	public static string PatchId => "thequeen_ecosphere_potion_target_node";
	public static string Description => "Ecosphere bottle: show capture preview on enemy targeting";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NPotionHolder), "TargetNode", new[] { typeof(TargetType) }),
	];

	public static void Prefix(NPotionHolder __instance, TargetType targetType)
	{
		if (__instance.Potion?.Model is not EcosphereBottle || targetType != TargetType.AnyEnemy
		    || !CombatManager.Instance.IsInProgress)
		{
			return;
		}

		Player owner = __instance.Potion.Model.Owner;
		EcosphereBottleTargetingPreview.Begin(owner);
	}
}

internal sealed class EcosphereBottleTargetingPreview_NTargetManager_FinishTargeting_Patch : IPatchMethod
{
	public static string PatchId => "thequeen_ecosphere_finish_targeting";
	public static string Description => "Ecosphere bottle: end capture preview";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NTargetManager), "FinishTargeting", new[] { typeof(bool) }),
	];

	public static void Postfix()
	{
		EcosphereBottleTargetingPreview.End();
	}
}
