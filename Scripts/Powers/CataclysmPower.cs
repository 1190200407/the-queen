using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>灾变（可叠加）：下 N 次受到未被格挡的攻击伤害时，随机获得等同于该伤害的毒/灾厄/消亡之一。</summary>
public sealed class CataclysmPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<PoisonPower>(),
		HoverTipFactory.FromPower<DoomPower>(),
		HoverTipFactory.FromPower<DemisePower>(),
	];

	public override async Task AfterDamageReceived(
		PlayerChoiceContext choiceContext,
		Creature target,
		DamageResult result,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource)
	{
		if (target != base.Owner || !props.IsPoweredAttack() || result.UnblockedDamage <= 0)
		{
			return;
		}

		Creature? applier = base.Applier;
		if (applier?.Player is not { } ownerPlayer)
		{
			await PowerCmd.Decrement(this);
			return;
		}

		decimal stacks = result.UnblockedDamage;
		Flash();
		if (stacks > 0m)
		{
			await QueenCardCmd.ApplyRandomTriadDebuff(choiceContext, ownerPlayer, target, applier, cardSource, stacks);
		}

		await PowerCmd.Decrement(this);
	}
}
