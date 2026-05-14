using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 与原版 <see cref="MegaCrit.Sts2.Core.Models.Powers.DieForYouPower"/> 一致：吸收对主人的未格挡攻击伤害。
/// 沉睡（灯槽首意图为沉睡、或 <see cref="AmalgamEmergencySleepForcedActionModel"/>）时不改目标。
/// 被击杀时不移出战斗：切沉睡动画与沉睡行动；下回合玩家侧 <see cref="FriendlyAmalgam.AfterTurnEnd"/> 先结算该沉睡（<see cref="FriendlyAmalgamCmd.ApplyDeathSleepReviveStatsAsync"/> 重算 Max 并置当前为 1，无 Heal 音效），本回合末不执行灯槽意图。
/// <see cref="FriendlyAmalgamCmd.Summon"/> 走「已有尸体」复活加血时则立刻结束击倒沉睡（不占回合末沉睡）。
/// </summary>
public sealed class AmalgamDieForYouPower : QueenPowerModel
{
	private sealed class Data
	{
		public bool AwaitingDeathSleepRevive;
	}

	protected override object? InitInternalData() => new Data();

	internal bool IsAwaitingDeathSleepRevive => GetInternalData<Data>().AwaitingDeathSleepRevive;

    public override bool ShouldPlayVfx => false;

    /// <summary><see cref="FriendlyAmalgamCmd.Summon"/> 复用场上已死聚合体并加血时调用，取消回合末沉睡占位。</summary>
    internal void WakeImmediatelyAfterSummonRevive()
	{
		GetInternalData<Data>().AwaitingDeathSleepRevive = false;
	}

	/// <summary>玩家回合末执行「击倒沉睡」行动：1 血复活（不走 <see cref="CreatureCmd.Heal"/>，治疗音效改由 <see cref="FriendlyAmalgamCmd.Summon"/> 铺血路径负责）。</summary>
	internal async Task ExecuteDeathSleepReviveSilentlyAsync(PlayerChoiceContext choiceContext, Creature creature)
	{
		_ = choiceContext;
		Data data = GetInternalData<Data>();
		if (!data.AwaitingDeathSleepRevive || creature != base.Owner || !creature.IsDead)
		{
			return;
		}

		await FriendlyAmalgamCmd.ApplyDeathSleepReviveStatsAsync(creature);
		data.AwaitingDeathSleepRevive = false;
		if (creature.Monster is FriendlyAmalgam amalgam)
		{
			amalgam.ClearForcedAction();
		}

		FriendlyAmalgamCmd.TryRefreshIntentTorchVisuals(creature);
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal unblockedDamage, ValueProp props, Creature? dealer)
	{
		if (target != base.Owner.PetOwner?.Creature)
		{
			return target;
		}

		if (base.Owner.IsDead)
		{
			return target;
		}

		if (!props.HasFlag(ValueProp.Move) || props.HasFlag(ValueProp.Unpowered))
		{
			return target;
		}

		if (base.Owner.Monster is not FriendlyAmalgam amalgam || amalgam.IsSleeping())
		{
			return target;
		}

        if (base.CombatState is { } combatState && unblockedDamage > 0m)
        {
            // 聚合体吸收未格挡伤害时，额外触发“被命中”事件，供胆小等能力使用。
            // cardSource 在该 hook 阶段不可得（原版 Hook.ModifyUnblockedDamageTarget 不传 cardSource）。
            _ = FriendlyAmalgamHook.AfterHit(combatState, base.Owner, unblockedDamage, props, dealer, cardSource: null);
        }

		return base.Owner;
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		_ = deathAnimLength;
		if (creature != base.Owner || wasRemovalPrevented || base.Owner.Monster is not FriendlyAmalgam amalgam)
		{
			return;
		}

		GetInternalData<Data>().AwaitingDeathSleepRevive = true;
		await CreatureCmd.TriggerAnim(creature, "Sleep", 0f);
		await amalgam.FallAsleep(FriendlyAmalgam.SleepReason.Dead);
		await amalgam.BeginForcedAction(new AmalgamEmergencySleepForcedActionModel(0m));
	}

	public override bool ShouldAllowHitting(Creature creature)
	{
		if (creature != base.Owner)
		{
			return true;
		}

		Data data = GetInternalData<Data>();
		if (creature.IsAlive)
		{
			return !data.AwaitingDeathSleepRevive;
		}

		// 已死：默认 false（不可选中/不可吃牌等）。但 FriendlyAmalgamCmd 会先上本能力再上 AmalgamEvolutionaryThirstPower；
		// PowerCmd.Apply 会查 Creature.CanReceivePowers → Hook.ShouldAllowHitting，若此处恒 false 则第二段 Apply 永远失败。
		if (creature.GetPower<AmalgamEvolutionaryThirstPower>() == null)
		{
			return true;
		}

		return false;
	}

	public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
	{
		if (creature != base.Owner)
		{
			return true;
		}

		return false;
	}

	public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;
}

