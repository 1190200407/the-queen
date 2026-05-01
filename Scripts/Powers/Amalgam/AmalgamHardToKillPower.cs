using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 难以杀灭（聚合体版）：聚合体每次受到的伤害与生命减少不会超过 <see cref="PowerModel.Amount"/> 点；持续 1 回合（敌方回合结束后移除）。
/// （数值与单次结算参考原版 <see cref="MegaCrit.Sts2.Core.Models.Powers.HardToKillPower"/>。）
/// </summary>
public sealed class AmalgamHardToKillPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://images/atlases/power_atlas.sprites/hard_to_kill_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/hard_to_kill_power.png";

    
    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner || amount <= 0m || Amount <= 0m)
        {
            return amount;
        }
 
        return decimal.Min(amount, Amount);
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        Flash();
        await Task.CompletedTask;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        _ = choiceContext;
        if (side != base.Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}
