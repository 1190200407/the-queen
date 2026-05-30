using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 「魂缚誓约」：每名玩家每回合最多手动打出 1 张 <see cref="Bound"/> 牌；有魂灯层数时可无视该限制。
/// </summary>
internal static class BindingOathPatchState
{
	private static readonly Dictionary<ulong, bool> BoundCardPlayedThisTurn = new();

	private static AbstractModel? _bindingOathPreventer;

	internal static AbstractModel BindingOathPreventer =>
		_bindingOathPreventer ??= ModelDb.Power<BindingOathPreventerPower>();

	internal static void ApplyShouldPlayBlock(
		CardModel card,
		AutoPlayType autoPlayType,
		ref bool __result,
		ref AbstractModel? preventer)
	{
		if (!__result || autoPlayType != AutoPlayType.None)
		{
			return;
		}

		Player? owner = card.Owner;

		if (card.Affliction is not Bound)
		{
			return;
		}

		Creature creature = owner.Creature;
		SoulLampPower? lamp = creature.GetPower<SoulLampPower>();
		if (lamp != null && lamp.Amount > 0)
		{
			return;
		}

		if (!BoundCardPlayedThisTurn.TryGetValue(owner.NetId, out bool played) || !played)
		{
			return;
		}

		__result = false;
		preventer = BindingOathPreventer;
	}

	internal static void NoteBoundCardPlayed(CardPlay cardPlay)
	{
		CardModel card = cardPlay.Card;
		if (card.IsDupe)
		{
			return;
		}

		Player? owner = card.Owner;
		if (owner == null || card.Owner.Creature != owner.Creature)
		{
			return;
		}

		if (card.Affliction is not Bound)
		{
			return;
		}

		BoundCardPlayedThisTurn[owner.NetId] = true;
	}

	internal static void ResetPerTurnFlags(CombatState combatState)
	{
		foreach (Player p in combatState.Players)
		{
			BoundCardPlayedThisTurn[p.NetId] = false;
		}
	}

	internal static void ClearForNewCombat()
	{
		BoundCardPlayedThisTurn.Clear();
	}
}

internal sealed class BindingOathHookShouldPlayPatch : IPatchMethod
{
	public static string PatchId => "thequeen_binding_oath_should_play";
	public static string Description => "Binding oath: limit bound card plays per turn";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(Hook), nameof(Hook.ShouldPlay)),
	];

	public static void Postfix(
		CombatState combatState,
		CardModel card,
		ref AbstractModel? preventer,
		AutoPlayType autoPlayType,
		ref bool __result)
	{
		_ = combatState;
		BindingOathPatchState.ApplyShouldPlayBlock(card, autoPlayType, ref __result, ref preventer);
	}
}

internal sealed class BindingOathHookBeforeCardPlayedPatch : IPatchMethod
{
	public static string PatchId => "thequeen_binding_oath_before_card_played";
	public static string Description => "Binding oath: track bound card played";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(Hook), nameof(Hook.BeforeCardPlayed)),
	];

	public static async Task Postfix(Task __result, CombatState combatState, CardPlay cardPlay)
	{
		_ = combatState;
		await __result;
		BindingOathPatchState.NoteBoundCardPlayed(cardPlay);
	}
}

internal sealed class BindingOathHookBeforeTurnEndPatch : IPatchMethod
{
	public static string PatchId => "thequeen_binding_oath_before_turn_end";
	public static string Description => "Binding oath: reset per-turn flags";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(Hook), nameof(Hook.BeforeTurnEnd)),
	];

	public static async Task Postfix(Task __result, CombatState combatState, CombatSide side)
	{
		_ = side;
		await __result;
		BindingOathPatchState.ResetPerTurnFlags(combatState);
	}
}

internal sealed class BindingOathHookBeforeCombatStartPatch : IPatchMethod
{
	public static string PatchId => "thequeen_binding_oath_before_combat_start";
	public static string Description => "Binding oath: clear combat state";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(Hook), nameof(Hook.BeforeCombatStart)),
	];

	public static async Task Postfix(Task __result, IRunState runState, CombatState? combatState)
	{
		_ = runState;
		_ = combatState;
		await __result;
		BindingOathPatchState.ClearForNewCombat();
	}
}
