using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>单次怪物 <see cref="MegaCrit.Sts2.Core.Commands.Builders.AttackCommand"/> 内多段 <see cref="CreatureCmd.Damage"/> 共用：转移目标、推迟到 <see cref="MegaCrit.Sts2.Core.Hooks.Hook.AfterAttack"/> 再移除能力。</summary>
internal sealed class MindControlAttackFrame
{
	internal Creature? RedirectDealer;
	internal Creature? SharedRedirect;
	internal readonly HashSet<Creature> AppliersConsumed = [];
	internal readonly List<MindControlPower> PowersToRemove = [];

	internal void FlushRemovals()
	{
		if (PowersToRemove.Count == 0)
		{
			return;
		}

		Task removeAll = Task.WhenAll(PowersToRemove.Select(static p => PowerCmd.Remove(p)));
		TaskHelper.RunSafely(removeAll);
	}
}

/// <summary>
/// 精神控制：挂在<strong>被施加精神控制的敌人</strong>上，<see cref="IsInstanced"/> 可叠多条；
/// <see cref="AfterApplied"/> 与 <see cref="AfterPowerAmountChanged"/> 会刷新宿主怪物意图，便于意图数字与 <see cref="MindControlAttackIntentGetSingleDamagePatch"/> 等在能力变化后及时更新。
/// </summary>
public sealed class MindControlPower : QueenPowerModel
{
	private const string ApplierPlayerNameKey = "ApplierPlayerName";

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override bool IsInstanced => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar(ApplierPlayerNameKey)];

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		_ = applier;
		_ = cardSource;
		if (Applier?.Player is { } player)
		{
			((StringVar)base.DynamicVars[ApplierPlayerNameKey]).StringValue = PlatformUtil.GetPlayerName(
				RunManager.Instance.NetService.Platform,
				player.NetId);
		}

		TryRefreshOwnerMonsterIntent();
		return Task.CompletedTask;
	}

	/// <summary>宿主身上任意能力层数变化时由 <c>Hook.AfterPowerAmountChanged</c> 广播；用于刷新意图（含依赖 <c>targets</c> 的预览）。</summary>
	public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		_ = amount;
		_ = applier;
		_ = cardSource;
		if (power.Owner != base.Owner)
		{
			return Task.CompletedTask;
		}

		TryRefreshOwnerMonsterIntent();
		return Task.CompletedTask;
	}

	/// <summary>与 <see cref="MegaCrit.Sts2.Core.Nodes.Combat.NCreature.RefreshIntents"/> 一致，重新拉取意图 UI。</summary>
	private void TryRefreshOwnerMonsterIntent()
	{
		Creature? owner = base.Owner;
		if (owner?.CombatState is not CombatState combatState || !owner.IsEnemy || owner.Monster == null || !owner.IsAlive)
		{
			return;
		}

		if (NCombatRoom.Instance?.GetCreatureNode(owner) is not { } node)
		{
			return;
		}

		TaskHelper.RunSafely(node.RefreshIntents());
	}

	/// <summary>由 <see cref="MindControlDamagePatch"/> 在 <see cref="CreatureCmd.Damage"/> 前缀中调用。</summary>
	internal static bool TryApplyRedirectToTargets(
		List<Creature> targets,
		CombatState combatState,
		Creature dealer,
		ValueProp props,
		CardModel? cardSource,
		MindControlAttackFrame? attackFrame)
	{
		if (!MindControlDamagePatch.IsInsideMonsterAttackCommand)
		{
			return false;
		}

		if (cardSource is not null)
		{
			return false;
		}

		if (!props.HasFlag(ValueProp.Move) || props.HasFlag(ValueProp.Unpowered))
		{
			return false;
		}

		List<MindControlPower> allInstances = dealer.GetPowerInstances<MindControlPower>().ToList();
		if (allInstances.Count == 0)
		{
			return false;
		}

		HashSet<Creature>? appliersInTargets = CollectAppliersInTargets(targets, allInstances);
		if (appliersInTargets is null || appliersInTargets.Count == 0)
		{
			return false;
		}

		// 同一 AttackCommand 内后续段：复用首段随机出的转移目标，且推迟移除到 AfterAttack。
		if (attackFrame?.SharedRedirect is { } cached
		    && ReferenceEquals(attackFrame.RedirectDealer, dealer))
		{
			if (!ApplySharedRedirectToTargets(targets, appliersInTargets, cached))
			{
				return false;
			}

			QueueDeferredRemovals(attackFrame, allInstances, appliersInTargets);
			return true;
		}

		Creature? rngSourceApplier = targets.FirstOrDefault(appliersInTargets.Contains);
		if (rngSourceApplier is null)
		{
			return false;
		}

		Creature sharedRedirect = ResolveSharedRedirect(combatState, dealer, rngSourceApplier);
		if (!ApplySharedRedirectToTargets(targets, appliersInTargets, sharedRedirect))
		{
			return false;
		}

		if (attackFrame is not null)
		{
			attackFrame.RedirectDealer = dealer;
			attackFrame.SharedRedirect = sharedRedirect;
			QueueDeferredRemovals(attackFrame, allInstances, appliersInTargets);
			return true;
		}

		// 无帧时（理论上不应在受控攻击内发生）仍立即移除，避免能力泄漏。
		ImmediateRemoveOnePowerPerApplier(dealer, appliersInTargets);
		return true;
	}

	private static HashSet<Creature>? CollectAppliersInTargets(List<Creature> targets, List<MindControlPower> allInstances)
	{
		HashSet<Creature> appliersInTargets = [];
		foreach (MindControlPower p in allInstances)
		{
			Creature? a = p.Applier;
			if (a is null || !a.IsAlive)
			{
				continue;
			}

			if (targets.Contains(a))
			{
				appliersInTargets.Add(a);
			}
		}

		return appliersInTargets.Count == 0 ? null : appliersInTargets;
	}

	private static bool ApplySharedRedirectToTargets(List<Creature> targets, HashSet<Creature> appliersInTargets, Creature sharedRedirect)
	{
		bool changed = false;
		for (int i = 0; i < targets.Count; i++)
		{
			if (appliersInTargets.Contains(targets[i]))
			{
				targets[i] = sharedRedirect;
				changed = true;
			}
		}

		return changed;
	}

	private static void QueueDeferredRemovals(MindControlAttackFrame frame, List<MindControlPower> allInstances, HashSet<Creature> appliersInTargets)
	{
		foreach (Creature applier in appliersInTargets)
		{
			if (!frame.AppliersConsumed.Add(applier))
			{
				continue;
			}

			MindControlPower? one = allInstances.FirstOrDefault(p => p.Applier == applier);
			if (one is not null)
			{
				frame.PowersToRemove.Add(one);
			}
		}
	}

	private static void ImmediateRemoveOnePowerPerApplier(Creature dealer, HashSet<Creature> appliersInTargets)
	{
		List<MindControlPower> toRemove = [];
		foreach (Creature applier in appliersInTargets)
		{
			MindControlPower? one = dealer.GetPowerInstances<MindControlPower>().FirstOrDefault(p => p.Applier == applier);
			if (one is not null)
			{
				toRemove.Add(one);
			}
		}

		if (toRemove.Count > 0)
		{
			Task removeAll = Task.WhenAll(toRemove.Select(static p => PowerCmd.Remove(p)));
			TaskHelper.RunSafely(removeAll);
		}
	}

	private static Creature ResolveSharedRedirect(CombatState combatState, Creature dealer, Creature rngSourceApplier)
	{
		List<Creature> candidates = combatState.HittableEnemies.Where(e => e != dealer && e.IsAlive).ToList();
		if (candidates.Count == 0)
		{
			return dealer;
		}

		Player? applierPlayer = rngSourceApplier.Player;
		Rng? combatRng = applierPlayer?.RunState.Rng.CombatTargets;
		return combatRng is null
			? candidates[0]
			: combatRng.NextItem(candidates) ?? candidates[0];
	}
}
