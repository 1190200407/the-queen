using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>本回合技能牌耗能降低（减免量 = Amount）；回合结束时移除。</summary>
public sealed class SkillCostMinusOneThisTurnPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner?.Creature != base.Owner || card.EnergyCost.CostsX)
        {
            return false;
        }

        if (card.Type != CardType.Skill)
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

        modifiedCost = Math.Max(0m, originalCost - base.Amount);
        return modifiedCost != originalCost;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        _ = choiceContext;
        if (side == base.Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}

