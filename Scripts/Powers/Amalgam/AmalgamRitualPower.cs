using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>
/// 仪式（聚合体版）：<see cref="FriendlyAmalgam"/> 在玩家回合末完成行动序列后，获得等同于层数的 <see cref="StrengthPower"/>。
/// 不用 <see cref="PowerModel.AfterSideTurnEnd"/>：该 hook 在 <see cref="FriendlyAmalgam.AfterSideTurnEnd"/> 执行意图<strong>之前</strong>就会跑到聚合体上的 Power。
/// </summary>
public sealed class AmalgamRitualPower : QueenPowerModel, IAmalgamEventListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/ritual_power.tres";

    public override string? CustomBigIconPath => "res://images/powers/ritual_power.png";

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    public async Task AfterAmalgamTurnEnd(ICombatState combatState, Creature amalgam)
    {
        _ = combatState;
        if (amalgam != base.Owner || !amalgam.IsAlive || Amount <= 0m)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<StrengthPower>(
            new ThrowingPlayerChoiceContext(),
            base.Owner,
            base.Amount,
            base.Owner,
            null);
    }
}
