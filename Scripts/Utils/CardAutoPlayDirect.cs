using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 与 <see cref="CardCmd.AutoPlay"/> 对齐的实现（含 <see cref="TargetType.AnyPlayer"/> 随机目标），但不走经 BaseLib 补丁的入口，
/// 避免其引用已移除的 <c>CombatState</c> 时在 JIT 抛出 <see cref="TypeLoadException"/>。
/// </summary>
internal static class CardAutoPlayDirect
{
	internal static async Task AutoPlayAsync(
		PlayerChoiceContext choiceContext,
		CardModel card,
		Creature? target,
		AutoPlayType type,
		bool skipXCapture = false,
		bool skipCardPileVisuals = false)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}

		Player? owner = card.Owner;
		ICombatState? combatStateOrNull = card.CombatState ?? owner?.Creature?.CombatState;
		if (owner == null || combatStateOrNull == null)
		{
			return;
		}

		ICombatState combatState = combatStateOrNull;
		if (card.Keywords.Contains(CardKeyword.Unplayable))
		{
			await MoveToResultPileWithoutPlayingAsync(choiceContext, card);
			return;
		}

		// sts2 中 GetPlayerDialogueLine 为 internal 扩展，模组无法调用；被拦时仅落结果堆即可。
		if (!Hook.ShouldPlay(combatState, card, out AbstractModel? _, type))
		{
			await MoveToResultPileWithoutPlayingAsync(choiceContext, card);
			return;
		}

		if (card.TargetType == TargetType.AnyEnemy)
		{
			if (target == null)
			{
				target = owner.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies);
			}

			if (target == null)
			{
				await MoveToResultPileWithoutPlayingAsync(choiceContext, card);
				return;
			}
		}
		else if (card.TargetType == TargetType.AnyAlly)
		{
			Creature selfCreature = owner.Creature;
			IEnumerable<Creature> items = combatState.Allies.Where(c => c != null && c.IsAlive && c.IsPlayer && c != selfCreature);
			if (target == null)
			{
				target = owner.RunState.Rng.CombatTargets.NextItem(items);
			}

			if (target == null)
			{
				await MoveToResultPileWithoutPlayingAsync(choiceContext, card);
				return;
			}
		}
		else if (card.TargetType == TargetType.AnyPlayer)
		{
			IEnumerable<Creature> items = combatState.PlayerCreatures.Where(static c => c != null && c.IsAlive);
			if (target == null)
			{
				target = owner.RunState.Rng.CombatTargets.NextItem(items);
			}

			if (target == null)
			{
				await MoveToResultPileWithoutPlayingAsync(choiceContext, card);
				return;
			}
		}

		if (!card.IsDupe && owner.PlayerCombatState is { } playerCombatState)
		{
			if (card.EnergyCost.CostsX && !skipXCapture)
			{
				card.EnergyCost.CapturedXValue = playerCombatState.Energy;
			}

			if (card.HasStarCostX)
			{
				card.LastStarsSpent = playerCombatState.Stars;
			}
			else
			{
				card.LastStarsSpent = Math.Max(0, card.GetStarCostWithModifiers());
			}
		}

		if (card.Pile == null)
		{
			await CardPileCmd.Add(card, PileType.Play);
		}

		if (!skipCardPileVisuals)
		{
			_ = TaskHelper.RunSafely(card.OnEnqueuePlayVfx(target));
		}

		await Hook.BeforeCardAutoPlayed(combatState, card, target, type);
		ResourceInfo resources = new ResourceInfo
		{
			EnergySpent = 0,
			EnergyValue = card.EnergyCost.GetAmountToSpend(),
			StarsSpent = 0,
			StarValue = Math.Max(0, card.GetStarCostWithModifiers())
		};
		await card.OnPlayWrapper(choiceContext, target, isAutoPlay: true, resources, skipCardPileVisuals);
	}

	private static async Task MoveToResultPileWithoutPlayingAsync(PlayerChoiceContext choiceContext, CardModel card)
	{
		await CardPileCmd.Add(card, PileType.Play);
		await card.MoveToResultPileWithoutPlaying(choiceContext);
	}
}
