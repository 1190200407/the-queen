using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 火炬头聚合体替主人承受「攻击」类未格挡伤害（与 <see cref="MegaCrit.Sts2.Core.Models.Powers.DieForYouPower"/> 一致用 <see cref="ValuePropExtensions.IsPoweredAttack"/>）。
/// 以聚合体<strong>即将执行的 MoveState</strong>判定沉睡（首意图为 <see cref="AmalgamSleepIntent"/>）或紧急避险强制沉睡时不承伤；承伤量不超过使自身保留至少 <see cref="MinHpAfterBodyguard"/> 点生命，溢出由 <see cref="AmalgamBodyguardSpillDamagePatch"/> 再打给主人。
/// </summary>
public sealed class AmalgamDieForYouPower : QueenPowerModel
{
	public const int MinHpAfterBodyguard = 1;

	/// <summary>防止溢出承伤再次改目标，造成递归。</summary>
	[ThreadStatic]
	public static bool SuppressBodyguardRedirect;

	private bool _redirectingFromOwner;

	private decimal _spillPending;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override bool ShouldPlayVfx => false;

	public override Creature ModifyUnblockedDamageTarget(Creature target, decimal _, ValueProp props, Creature? __)
	{
		_redirectingFromOwner = false;
		if (SuppressBodyguardRedirect)
		{
			return target;
		}

		if (target != base.Owner.PetOwner?.Creature)
		{
			return target;
		}

		if (base.Owner.IsDead)
		{
			return target;
		}

		if (!props.IsPoweredAttack())
		{
			return target;
		}

		if (base.Owner.Monster is not FriendlyAmalgam amalgam || amalgam.IsBodyguardSleeping())
		{
			return target;
		}

		_redirectingFromOwner = true;
		return base.Owner;
	}

	public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (SuppressBodyguardRedirect || !_redirectingFromOwner || target != base.Owner)
		{
			return amount;
		}

		int cap = System.Math.Max(0, target.CurrentHp - MinHpAfterBodyguard);
		decimal capped = System.Math.Min(amount, cap);
		_spillPending = amount - capped;
		return capped;
	}

	internal decimal ConsumeSpillAndClearRedirect()
	{
		_redirectingFromOwner = false;
		decimal s = _spillPending;
		_spillPending = 0m;
		return s;
	}

	public override bool ShouldAllowHitting(Creature creature) => creature.IsAlive;

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
