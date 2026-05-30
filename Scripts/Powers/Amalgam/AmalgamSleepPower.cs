using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 睡觉：获得时令聚合体进入沉睡；回合开始时层数 -1；层数清空或被移除时令聚合体苏醒。
/// </summary>
public sealed class AmalgamSleepPower : QueenPowerModel, IAmalgamEventListener
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 复用原版沉睡图标。
    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/asleep_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/asleep_power.png";

    private bool appliedSleep = false;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        _ = amount;
        _ = applier;
        _ = cardSource;

        if (power != this || base.Owner.Monster is not FriendlyAmalgam amalgam)
        {
            return;
        }

        if (Amount > 0m && !appliedSleep)
        {
            appliedSleep = true;
            await amalgam.FallAsleep(FriendlyAmalgam.SleepReason.Power);
            return;
        }

        if (Amount <= 0m && appliedSleep)
        {
            appliedSleep = false;
            await amalgam.WakeUp(FriendlyAmalgam.SleepReason.Power);
        }
    }

    public async Task AfterAmalgamTurnEnd(CombatState combatState, Creature amalgam)
    {
        await PowerCmd.Decrement(this);
    }
}

