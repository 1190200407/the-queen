using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// Friendly-amalgam version of Die For You.
/// It redirects unblocked move damage from the queen to the amalgam while the amalgam is awake.
/// </summary>
public sealed class AmalgamDieForYouPower : QueenPowerModel
{
	private sealed class Data
	{
		public bool AwaitingDeathSleepRevive;
		public int PendingRedirectedHits;
	}

	protected override object? InitInternalData() => new Data();

	internal bool IsAwaitingDeathSleepRevive => GetInternalData<Data>().AwaitingDeathSleepRevive;

	public override bool ShouldPlayVfx => false;

	internal void WakeImmediatelyAfterSummonRevive()
	{
		GetInternalData<Data>().AwaitingDeathSleepRevive = false;
	}

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
		_ = unblockedDamage;
		_ = dealer;

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

		if (unblockedDamage > 0m)
		{
			GetInternalData<Data>().PendingRedirectedHits++;
		}

		return base.Owner;
	}

	public override async Task AfterDamageReceived(
		PlayerChoiceContext choiceContext,
		Creature target,
		DamageResult result,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource)
	{
		_ = choiceContext;
		Data data = GetInternalData<Data>();
		if (data.PendingRedirectedHits <= 0)
		{
			return;
		}

		data.PendingRedirectedHits--;
		if (target != base.Owner || result.Receiver != base.Owner || result.UnblockedDamage <= 0m)
		{
			return;
		}

		if (!props.HasFlag(ValueProp.Move) || props.HasFlag(ValueProp.Unpowered))
		{
			return;
		}

		if (base.CombatState is not { } combatState)
		{
			return;
		}

		await FriendlyAmalgamHook.AfterHit(combatState, base.Owner, result.UnblockedDamage, props, dealer, cardSource);
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

		// Allow the initial core-power setup on the retained corpse shell.
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
