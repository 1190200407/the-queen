using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
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

		if (!SoulLampResources.HasAny(card.Owner))
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

	// 资源类能力：不用 Buff，避免「清除增益」类效果误删魂灯；层数变化走 SoulLampHook。
	public override PowerType Type => PowerType.None;

	// 女王在能量指示器上显示魂灯；其他角色仍走能力栏。
	protected override bool IsVisibleInternal => !IsLocalQueenOwner();

	public override bool ShouldPlayVfx => !IsLocalQueenOwner();

    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomBigIconPath => "res://TheQueen/images/powers/big/soul_lamp.png";
	public override string? CustomIconPath => "res://TheQueen/images/powers/soul_lamp.png";

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
		if (card.Owner?.Creature != base.Owner || !SoulLampResources.HasAny(card.Owner))
		{
			return false;
		}

		if (card.Affliction is not Bound)
		{
			return false;
		}

		return card.Pile?.Type is PileType.Hand or PileType.Play;
	}

	private bool IsLocalQueenOwner()
	{
		Player? player = base.Owner?.Player;
		return player?.Character is QueenCharacter && LocalContext.IsMe(player);
	}
}
