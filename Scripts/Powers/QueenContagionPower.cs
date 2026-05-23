using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>传染：当你获得力量时，所有敌人获得双倍力量。</summary>
public sealed class QueenContagionPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[HoverTipFactory.FromPower<TaintedPower>()];

	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (amount <= 0m || power is not TaintedPower || power.Owner != base.Owner)
		{
			return;
		}

		if (base.CombatState is not { } combatState)
		{
			return;
		}

		decimal spreadAmount = amount * Amount;
		Flash();
		foreach (Creature enemy in combatState.Enemies)
		{
			if (enemy.IsAlive)
			{
				await PowerCmd.Apply<TaintedPower>(choiceContext, enemy, spreadAmount, base.Owner, cardSource);
			}
		}
	}
}
