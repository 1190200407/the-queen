using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>盾墙：回合开始时，若聚合体存活，则你获得 <see cref="PowerModel.Amount"/> 点格挡。</summary>
public sealed class AmalgamRampartPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 复用原版 Rampart 的图标资源。
    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/rampart_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/rampart_power.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _ = choiceContext;
        if (!base.Owner.IsAlive || player != base.Owner.PetOwner || base.Owner.CombatState is null)
        {
            return;
        }

        if (Amount <= 0m)
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(player.Creature, Amount, ValueProp.Unpowered, null);
    }
}

