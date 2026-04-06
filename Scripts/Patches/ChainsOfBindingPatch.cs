using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;

namespace ComicChess.TheQueen;

/// <summary>
/// 魂缚锁链与女王 <see cref="BindingOathPatch"/>：女王侧出牌限制由誓约 Patch 负责。
/// 锁链改为每回合最多对 <see cref="ChainsOfBindingPower.Amount"/> 张抽到的牌施加魂缚（原版逻辑），
/// 回合结束时<strong>只清除这些牌</strong>上的魂缚，保留牌面自带魂缚。
/// </summary>
[HarmonyPatch]
internal static class ChainsOfBindingPatch
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
	private static async Task BeforeCombatStart_Postfix(Task __result, IRunState runState, CombatState? combatState)
	{
		_ = runState;
		_ = combatState;
		await __result;
		ChainsOfBindingBoundTracker.ResetCombat();
	}

	/// <summary>女王：锁链不限制魂缚出牌，由 <see cref="BindingOathPatch"/> 判断。</summary>
	[HarmonyPostfix]
	[HarmonyPatch(typeof(ChainsOfBindingPower), nameof(ChainsOfBindingPower.ShouldPlay))]
	private static void ShouldPlay_Postfix(CardModel card, ref bool __result)
	{
		if (card.Owner?.Character is QueenCharacter)
		{
			__result = true;
		}
	}

	/// <summary>女王：不维护锁链的 <c>boundCardPlayed</c>。</summary>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(ChainsOfBindingPower), nameof(ChainsOfBindingPower.BeforeCardPlayed))]
	private static bool BeforeCardPlayed_Prefix(ChainsOfBindingPower __instance, CardPlay cardPlay, ref Task __result)
	{
		_ = __instance;
		Player? owner = cardPlay.Card.Owner;
		if (owner?.Character is not QueenCharacter)
		{
			return true;
		}

		__result = Task.CompletedTask;
		return false;
	}

	/// <summary>用独立计数与牌集合替代原版「历史条目数」，避免与自带魂缚的结算混在一起。</summary>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(ChainsOfBindingPower), nameof(ChainsOfBindingPower.AfterCardDrawn))]
	private static bool AfterCardDrawn_Prefix(
		ChainsOfBindingPower __instance,
		PlayerChoiceContext choiceContext,
		CardModel card,
		bool fromHandDraw,
		ref Task __result)
	{
		_ = choiceContext;
		_ = fromHandDraw;
		__result = AfterCardDrawn_ChainsAsync(__instance, card);
		return false;
	}

	private static async Task AfterCardDrawn_ChainsAsync(ChainsOfBindingPower power, CardModel card)
	{
		Player? player = power.Owner?.Player;
		if (card.Owner != player || power.CombatState.CurrentSide != power.Owner.Side)
		{
			return;
		}

		ChainsOfBindingBoundTracker.RegisterCardDrawn(player);
		if (!ModelDb.Affliction<Bound>().CanAfflict(card))
		{
			return;
		}

		int cap = power.Amount;
		if (!ChainsOfBindingBoundTracker.CanApplyAnotherCard(player, cap))
		{
			return;
		}

		await CardCmd.AfflictAndPreview<Bound>(
			new List<CardModel> { card },
			power.Amount,
			CardPreviewStyle.None);
		ChainsOfBindingBoundTracker.RegisterChainsBoundCard(player, card);
	}

	/// <summary>仅在本侧回合结束时清理锁链登记的牌；不清消耗堆中的登记牌。</summary>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(ChainsOfBindingPower), nameof(ChainsOfBindingPower.BeforeTurnEnd))]
	private static bool BeforeTurnEnd_Prefix(
		ChainsOfBindingPower __instance,
		PlayerChoiceContext choiceContext,
		CombatSide side,
		ref Task __result)
	{
		_ = choiceContext;

		object? internalData = Traverse.Create(__instance).Field("_internalData").GetValue();
		if (internalData != null)
		{
			Traverse.Create(internalData).Field("boundCardPlayed").SetValue(false);
		}

		if (side == __instance.Owner.Side)
		{
			ChainsOfBindingBoundTracker.ClearChainsBoundsEndOfTurn(__instance.Owner.Player);
		}

		__result = Task.CompletedTask;
		return false;
	}
}
