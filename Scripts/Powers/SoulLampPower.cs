using System;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;

namespace ComicChess.TheQueen;

public sealed class SoulLampPower : QueenPowerModel
{
	internal static bool IsCardFreeBySoulLamp(CardModel? card)
	{
		if (card == null || card.EnergyCost.CostsX)
		{
			return false;
		}
		if (card.Owner?.Creature == null || card.Affliction is not Bound)
		{
			return false;
		}

		SoulLampPower? lamp = card.Owner.Creature.GetPower<SoulLampPower>();
		if (lamp == null || lamp.Amount <= 0)
		{
			return false;
		}

		switch (card.Pile?.Type)
		{
			case PileType.Hand:
			case PileType.Play:
				break;
			default:
				return false;
		}

		return card.EnergyCost.GetWithModifiers(CostModifiers.All) == 0;
	}

	public override PowerType Type => PowerType.None;

	// 女王在能量指示器上显示魂灯；其他角色仍走能力栏。
	protected override bool IsVisibleInternal => base.Owner?.Player?.Character is not QueenCharacter;

	public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomBigIconPath => "res://TheQueen/images/powers/big/soul_lamp.png";
	public override string? CustomIconPath => "res://TheQueen/images/powers/soul_lamp.png";

    // 引擎默认 Amount == 0 时会移除 Power。
    // 我们为了让状态栏还能显示“0层”，在最后一层被消耗时把数值跳到 -1，
    // 并重写 DisplayAmount 让它显示为 0，同时效果在 Amount <= 0 时失效。
    public override bool AllowNegative => true;

	public override int DisplayAmount => Math.Max(0, Amount);

	public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (!ShouldZeroBoundCardCost(card))
		{
			return false;
		}

		modifiedCost = default(decimal);
		return true;
	}

    public override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!ShouldZeroBoundCardCost(card))
        {
            return false;
        }

		modifiedCost = default(decimal);
		return true;
    }

	private bool ShouldZeroBoundCardCost(CardModel card)
	{
		if (base.Amount <= 0)
		{
			return false;
		}

		if (card.Owner?.Creature != base.Owner)
		{
			return false;
		}

		if (card.Affliction is not Bound)
		{
			return false;
		}

		return card.Pile?.Type is PileType.Hand or PileType.Play;
	}

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
		if (power != this || amount == 0m)
		{
			return;
		}

		Player? player = base.Owner?.Player;
		if (player == null)
		{
			return;
		}

		await QueenCardModel.BroadcastSoulLampAmountChange(player, amount, applier, cardSource);
		if (amount < 0m)
		{
			await MagicTimePower.TryAutoRefillSoulLamp(player);
		}

		NQueenEnergyCounter.TryRefresh(player);
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
					await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, -2m, null, null);
				}
				else
				{
					await PowerCmd.Decrement(this);
				}
			}
		}
	}
}