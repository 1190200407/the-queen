using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>
/// 不详之咒：每场战斗第一次由你方对敌人施加虚弱或易伤时，层数 +{BonusStacks}。
/// <see cref="ModifyPowerAmountGiven"/> 在卡牌预览中也会被调用，因此<strong>只改数值、不消耗次数</strong>；
/// 次数在 <see cref="AfterModifyingPowerAmountGiven"/>（仅真实 <see cref="MegaCrit.Sts2.Core.Commands.PowerCmd"/> 结算）中消耗。
/// </summary>

public sealed class OminousCurseRelic : QueenRelicModel
{
	private bool _firstDebuffBonusConsumed;

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("BonusStacks", 1m)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<VulnerablePower>(),
		HoverTipFactory.FromPower<WeakPower>(),
	];

	public override Task BeforeCombatStart()
	{
		_firstDebuffBonusConsumed = false;
		return Task.CompletedTask;
	}

	public override decimal ModifyPowerAmountGiven(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
	{
		if (_firstDebuffBonusConsumed || amount <= 0m)
		{
			return amount;
		}

		if (power is not WeakPower && power is not VulnerablePower)
		{
			return amount;
		}

		if (!IsGiverFromRelicOwner(giver) || target?.Side != CombatSide.Enemy)
		{
			return amount;
		}

		return amount + base.DynamicVars["BonusStacks"].BaseValue;
	}

	public override Task AfterModifyingPowerAmountGiven(PowerModel power)
	{
		ModelId weakId = ModelDb.Power<WeakPower>().Id;
		ModelId vulnId = ModelDb.Power<VulnerablePower>().Id;
		if (power.Id != weakId && power.Id != vulnId)
		{
			return Task.CompletedTask;
		}

		if (_firstDebuffBonusConsumed)
		{
			return Task.CompletedTask;
		}

		_firstDebuffBonusConsumed = true;
		Flash();
		return Task.CompletedTask;
	}

	private bool IsGiverFromRelicOwner(Creature giver)
	{
		if (giver.Side != CombatSide.Player)
		{
			return false;
		}

		if (giver == base.Owner.Creature)
		{
			return true;
		}

		if (giver.Player == base.Owner)
		{
			return true;
		}

		return giver.PetOwner == base.Owner;
	}
}
