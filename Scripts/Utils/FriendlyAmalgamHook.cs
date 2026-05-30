using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>友方聚合体扩展事件：行动后、苏醒后、以及造成伤害后的斩杀钩子。</summary>
public static class FriendlyAmalgamHook
{
	/// <summary>
	/// 聚合体对 <paramref name="damagedEnemy"/> 结算伤害后调用；若本次结算中将其斩杀，则触发
	/// <see cref="PredatoryAssimilationPower.TryGrantCaptureAfterAmalgamFatalKillAsync"/> 等基于斩杀的扩展。
	/// </summary>
	public static async Task AfterAmalgamDamagedCreature(
		CombatState combatState,
		PlayerChoiceContext choiceContext,
		Creature amalgam,
		Creature damagedEnemy,
		IEnumerable<DamageResult> damageResults)
	{
		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			if (item is IAmalgamEventListener listener)
			{
				await listener.OnAmalgamDamagedCreatureAsync(combatState, choiceContext, amalgam, damagedEnemy, damageResults);
			}
		}
	}

	/// <summary>聚合体完成一次行动后触发（用于先锋等效果）。</summary>
	public static async Task AfterAct(CombatState combatState, PlayerChoiceContext choiceContext, Creature amalgam)
	{
		if (amalgam.PetOwner is not Player)
		{
			return;
		}

		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			if (item is IAmalgamEventListener listener)
			{
				await listener.OnAmalgamActAsync(combatState, choiceContext, amalgam);
			}
		}
	}

	/// <summary>聚合体从沉睡状态恢复时触发（用于蛰伏再生等效果）。</summary>
	public static async Task AfterAwake(CombatState combatState, Creature amalgam)
	{
		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			if (item is IAmalgamEventListener listener)
			{
				await listener.OnAmalgamWakeFromSleepAsync(combatState, amalgam);
			}
		}
	}

	/// <summary>聚合体进入沉睡时触发。</summary>
	public static async Task AfterFallAsleep(CombatState combatState, Creature amalgam)
	{
		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			if (item is IAmalgamEventListener listener)
			{
				await listener.OnAmalgamFallAsleepAsync(combatState, amalgam);
			}
		}
	}

	/// <summary>聚合体逃跑后触发。</summary>
	public static async Task OnEscape(CombatState combatState, Creature amalgam)
	{
		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			if (item is IAmalgamEventListener listener)
			{
				await listener.OnAmalgamEscapeAsync(combatState, amalgam);
			}
		}
	}

	/// <summary>聚合体被命中（产生未格挡伤害）后触发。</summary>
	public static async Task AfterHit(
		CombatState combatState,
		Creature amalgam,
		decimal unblockedDamage,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource)
	{
		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			if (item is IAmalgamEventListener listener)
			{
				await listener.OnAmalgamHitAsync(combatState, amalgam, unblockedDamage, props, dealer, cardSource);
			}
		}
	}

	public static async Task AfterAmalgamTurnEnd(CombatState combatState, Creature amalgam)
	{
		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			if (item is IAmalgamEventListener listener)
			{
				await listener.AfterAmalgamTurnEnd(combatState, amalgam);
			}
		}
	}

	/// <summary>聚合体完成一次基准 <see cref="CreatureCmd.SetMaxHp"/> 后（开场壳或召唤/复活）。</summary>
	public static async Task OnAmalgamEnterCombat(
		CombatState combatState,
		PlayerChoiceContext? choiceContext,
		Player owner,
		Creature amalgam)
	{
		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			if (item is IAmalgamEventListener listener)
			{
				await listener.OnAmalgamEnterCombat(combatState, choiceContext, owner, amalgam);
			}
		}
	}

	/// <summary><see cref="FriendlyAmalgamCmd.LearnIntent"/> 完成写入之后。</summary>
	public static async Task AfterLearnIntent(
		CombatState combatState,
		PlayerChoiceContext choiceContext,
		Player amalgamOwner,
		Creature amalgam,
		AmalgamActionModel intent,
		AbstractModel? source)
	{
		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			if (item is IAmalgamEventListener listener)
			{
				await listener.AfterLearnIntent(combatState, choiceContext, amalgamOwner, amalgam, intent, source);
			}
		}
	}

	/// <summary><see cref="FriendlyAmalgamCmd.CombineIntent"/> 完成写入之后。</summary>
	public static async Task AfterCombineIntent(
		CombatState combatState,
		PlayerChoiceContext choiceContext,
		Player amalgamOwner,
		Creature amalgam,
		AmalgamActionModel intent,
		AbstractModel? source,
		AmalgamCompositeKey compositeKey)
	{
		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			if (item is IAmalgamEventListener listener)
			{
				await listener.AfterCombineIntent(combatState, choiceContext, amalgamOwner, amalgam, intent, source, compositeKey);
			}
		}
	}
}
