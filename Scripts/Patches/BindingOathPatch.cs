using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Runs;

namespace ComicChess.TheQueen;

/// <summary>
/// 女王「魂缚誓约」：每回合最多打出 1 张 <see cref="Bound"/> 牌；有魂灯层数时可无视该限制。
/// 原实现为 <c>BindingOathPower</c>，此处改为 Hook Patch，不在状态栏占用能力位。
/// </summary>
internal static class BindingOathPatchState
{
	private static readonly Dictionary<ulong, bool> BoundCardPlayedThisTurn = new();

	private static AbstractModel? _bindingOathPreventer;

	/// <summary><see cref="ModelDb.Power{T}"/> 的规范实例，只取一次供 ShouldPlay 气泡复用。</summary>
	internal static AbstractModel BindingOathPreventer =>
		_bindingOathPreventer ??= ModelDb.Power<BindingOathPreventerPower>();

	internal static void ApplyShouldPlayBlock(CardModel card, ref bool __result, ref AbstractModel? preventer)
	{
		if (!__result)
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

	internal static void NoteBoundCardPlayedIfQueen(CardPlay cardPlay)
	{
		CardModel card = cardPlay.Card;
		if (card.IsDupe)
		{
			return;
		}

		Player? owner = card.Owner;
		if (owner?.Character is not QueenCharacter || card.Owner.Creature != owner.Creature)
		{
			return;
		}

		if (card.Affliction is not Bound)
		{
			return;
		}

		BoundCardPlayedThisTurn[owner.NetId] = true;
	}

	/// <summary>与旧 Power 一致：每次回合结束阶段 Hook 都清零，避免跨侧回合残留。</summary>
	internal static void ResetQueenFlags(CombatState combatState)
	{
		foreach (Player p in combatState.Players)
		{
			if (p.Character is QueenCharacter)
			{
				BoundCardPlayedThisTurn[p.NetId] = false;
			}
		}
	}

	internal static void ClearForNewCombat()
	{
		BoundCardPlayedThisTurn.Clear();
		QueenScratchBonusTracker.Reset();
	}
}

[HarmonyPatch]
internal static class BindingOathPatch
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Hook), nameof(Hook.ShouldPlay))]
	private static void ShouldPlay_Postfix(
		CombatState combatState,
		CardModel card,
		ref AbstractModel? preventer,
		AutoPlayType autoPlayType,
		ref bool __result)
	{
		_ = combatState;
		_ = autoPlayType;
		BindingOathPatchState.ApplyShouldPlayBlock(card, ref __result, ref preventer);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed))]
	private static async Task BeforeCardPlayed_Postfix(Task __result, CombatState combatState, CardPlay cardPlay)
	{
		_ = combatState;
		await __result;
		BindingOathPatchState.NoteBoundCardPlayedIfQueen(cardPlay);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeTurnEnd))]
	private static async Task BeforeTurnEnd_Postfix(Task __result, CombatState combatState, CombatSide side)
	{
		_ = side;
		await __result;
		BindingOathPatchState.ResetQueenFlags(combatState);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
	private static async Task BeforeCombatStart_Postfix(Task __result, IRunState runState, CombatState? combatState)
	{
		_ = runState;
		await __result;
		BindingOathPatchState.ClearForNewCombat();
	}
}
