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
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 精神控制：挂在<strong>被施加精神控制的敌人</strong>上，<see cref="IsInstanced"/> 可叠多条；
/// <see cref="PowerModel.Applier"/> 为打出该牌的女王。同一击内多个 applier 命中时共用同一转移目标；每个被转移的 applier 各消耗一条实例。
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

		return Task.CompletedTask;
	}

	/// <summary>由 <see cref="MindControlDamagePatch"/> 在 <see cref="CreatureCmd.Damage"/> 前缀中调用。</summary>
	internal static bool TryApplyRedirectToTargets(
		List<Creature> targets,
		CombatState combatState,
		Creature dealer,
		ValueProp props,
		CardModel? cardSource)
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

		// 第一轮：攻击目标里出现了哪些 applier，对应哪些实例（仅用于判定；消耗在转移成功后按 applier 各删一条）。
		HashSet<Creature> appliersInTargets = new();
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

		if (appliersInTargets.Count == 0)
		{
			return false;
		}

		// 共用同一转移目标（按当前 targets 顺序取第一个命中的 applier 作为随机源，与联机一致）。
		Creature? rngSourceApplier = targets.FirstOrDefault(appliersInTargets.Contains);
		if (rngSourceApplier is null)
		{
			return false;
		}

		Creature sharedRedirect = ResolveSharedRedirect(combatState, dealer, rngSourceApplier);

		bool changed = false;
		for (int i = 0; i < targets.Count; i++)
		{
			if (appliersInTargets.Contains(targets[i]))
			{
				targets[i] = sharedRedirect;
				changed = true;
			}
		}

		if (!changed)
		{
			return false;
		}

		// 第二轮：每个被转移的 applier 删除恰好一条实例（同一 applier 多条实例时只删一条）。
		List<MindControlPower> toRemove = [];
		foreach (Creature applier in appliersInTargets)
		{
			MindControlPower? one = dealer.GetPowerInstances<MindControlPower>()
				.FirstOrDefault(p => p.Applier == applier);
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

		return true;
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
