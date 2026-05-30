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

/// <summary>单次怪物 <see cref="MegaCrit.Sts2.Core.Commands.Builders.AttackCommand"/> 内多段 <see cref="CreatureCmd.Damage"/> 共用：转移目标、推迟到 <see cref="MegaCrit.Sts2.Core.Hooks.Hook.AfterAttack"/> 再减层。</summary>
internal sealed class MindControlAttackFrame
{
	internal Creature? RedirectDealer;
	internal Creature? SharedRedirect;
	internal readonly List<MindControlPower> PowersToDecrement = [];

	internal void FlushDecrements()
	{
		if (PowersToDecrement.Count == 0)
		{
			return;
		}

		Task decrementAll = Task.WhenAll(PowersToDecrement.Select(static p => PowerCmd.Decrement(p)));
		TaskHelper.RunSafely(decrementAll);
	}
}

/// <summary>
/// 精神控制：挂在<strong>被施加精神控制的敌人</strong>上，每名施加者各一条实例（正式版无 <c>InstancedPerApplier</c>，由 <see cref="ApplyToTarget"/> 按 <see cref="PowerModel.Applier"/> 叠层）；
/// <see cref="AfterApplied"/> 与 <see cref="AfterPowerAmountChanged"/> 会刷新宿主怪物意图，便于意图数字与 <see cref="MindControlAttackIntentGetSingleDamagePatch"/> 等在能力变化后及时更新。
/// </summary>
public sealed class MindControlPower : QueenPowerModel
{
	private const string ApplierPlayerNameKey = "ApplierPlayerName";

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool IsInstanced => true;

	/// <summary>同一施加者再次施加时叠层，不同施加者各保留一条实例。</summary>
	internal static async Task<MindControlPower?> ApplyToTarget(
		Creature target,
		decimal amount,
		Creature applier,
		CardModel? cardSource)
	{
		MindControlPower? existing = target.GetPowerInstances<MindControlPower>()
			.FirstOrDefault(p => p.Applier == applier);
		if (existing is not null)
		{
			return await PowerCmd.ModifyAmount(existing, amount, applier, cardSource) == 0
				? null
				: existing;
		}

		return await PowerCmd.Apply<MindControlPower>(target, amount, applier, cardSource);
	}

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
		if (!MindControlDamagePatchState.IsInsideMonsterAttackCommand)
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

		List<MindControlPower> matchingInstances = GetMatchingInstances(targets, allInstances);
		if (matchingInstances.Count == 0)
		{
			return false;
		}

		HashSet<Creature> appliersInTargets = matchingInstances
			.Select(static p => p.Applier!)
			.ToHashSet();

		// 同一 AttackCommand 内后续段：复用首段随机出的转移目标，且推迟减层到 AfterAttack。
		if (attackFrame?.SharedRedirect is { } cached
		    && ReferenceEquals(attackFrame.RedirectDealer, dealer))
		{
			if (!ApplySharedRedirectToTargets(targets, appliersInTargets, cached))
			{
				return false;
			}

			QueueDeferredDecrements(attackFrame, matchingInstances);
			return true;
		}

		Creature? rngSourceApplier = matchingInstances[0].Applier;
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
			QueueDeferredDecrements(attackFrame, matchingInstances);
			return true;
		}

		// 无帧时（理论上不应在受控攻击内发生）仍立即减层，避免能力泄漏。
		ImmediateDecrementMatchingInstances(matchingInstances);
		return true;
	}

	private static List<MindControlPower> GetMatchingInstances(List<Creature> targets, List<MindControlPower> allInstances) =>
		allInstances
			.Where(p => p.Applier is { IsAlive: true } applier && targets.Contains(applier))
			.ToList();

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

	private static void QueueDeferredDecrements(MindControlAttackFrame frame, List<MindControlPower> matchingInstances)
	{
		foreach (MindControlPower power in matchingInstances)
		{
			if (!frame.PowersToDecrement.Contains(power))
			{
				frame.PowersToDecrement.Add(power);
			}
		}
	}

	private static void ImmediateDecrementMatchingInstances(List<MindControlPower> matchingInstances)
	{
		if (matchingInstances.Count == 0)
		{
			return;
		}

		Task decrementAll = Task.WhenAll(matchingInstances.Select(static p => PowerCmd.Decrement(p)));
		TaskHelper.RunSafely(decrementAll);
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
