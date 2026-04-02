using System;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;

namespace ComicChess.TheQueen;

public sealed class SoulLampPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	// 引擎默认 Amount == 0 时会移除 Power。
	// 我们为了让状态栏还能显示“0层”，在最后一层被消耗时把数值跳到 -1，
	// 并重写 DisplayAmount 让它显示为 0，同时效果在 Amount <= 0 时失效。
	public override bool AllowNegative => true;

	public override int DisplayAmount => Math.Max(0, Amount);

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (base.Amount <= 0)
		{
			return false;
		}

		if (card.Owner?.Creature != base.Owner)
		{
			return false;
		}
		if (!(card.Affliction is Bound))
		{
			return false;
		}

		// 只影响手牌/桌面上可打出的那种牌
		switch (card.Pile?.Type)
		{
			case PileType.Hand:
			case PileType.Play:
				break;
			default:
				return false;
		}

		modifiedCost = default(decimal);
		return true;
	}

	public override async Task BeforeCardPlayed(CardPlay cardPlay)
	{
		// 只在“手动打出”时消耗 1 层魂灯。
		if (cardPlay.IsAutoPlay)
		{
			return;
		}

		CardModel card = cardPlay.Card;
		if (card.Owner?.Creature != base.Owner)
		{
			return;
		}
		if (!(card.Affliction is Bound))
		{
			return;
		}
		if (cardPlay.IsFirstInSeries)
		{
			// 只消耗一次，避免同一张牌的多段重放多次扣层。
			switch (card.Pile?.Type)
			{
				case PileType.Hand:
				case PileType.Play:
					break;
				default:
					return;
			}
			if (base.Amount > 0)
			{
				// 避免 Amount 直接变成 0 导致 Power 被移除：
				// 从 1 -> -1（offset -2）并保持在状态栏显示 0。
				if (base.Amount == 1)
				{
					await PowerCmd.ModifyAmount(this, -2m, null, null);
				}
				else
				{
					await PowerCmd.Decrement(this);
				}
			}
		}
	}
}