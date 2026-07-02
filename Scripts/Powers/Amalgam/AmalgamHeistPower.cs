using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace ComicChess.TheQueen;

/// <summary>
/// 盗窃（聚合体通用资金池）：其它“抢钱”效果只需把金币累加到本 Power 层数；
/// 聚合体逃跑时会把累积金币加入战斗奖励。类名不可为 <c>HeistPower</c>（与原版能力 ModelId 冲突）。
/// </summary>
public sealed class AmalgamHeistPower : QueenPowerModel, IAmalgamEventListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(0)];

    // 复用原版 Heist 的图标资源。
    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/heist_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/heist_power.png";

    public Task OnAmalgamEscapeAsync(ICombatState combatState, Creature amalgam)
    {
        _ = combatState;
        if (amalgam != base.Owner || Amount <= 0m)
        {
            return Task.CompletedTask;
        }

        if (base.Owner.PetOwner is not Player player)
        {
            return Task.CompletedTask;
        }

        if (base.CombatState?.RunState.CurrentRoom is not CombatRoom combatRoom)
        {
            return Task.CompletedTask;
        }

        Flash();
        combatRoom.AddExtraReward(player, new GoldReward((int)Amount, player));
        return Task.CompletedTask;
    }
}
