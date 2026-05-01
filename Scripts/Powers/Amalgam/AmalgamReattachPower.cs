using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

/// <summary>
/// 接续：聚合体被击杀时，女王获得「下回合开始时召唤聚合体」；其最大生命为击倒前最大生命的一半。
/// </summary>
public sealed class AmalgamReattachPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
    
    public override string? CustomPackedIconPath => "res://images/atlases/power_atlas.sprites/reattach_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/reattach_power.png";

    public override async Task BeforeDeath(Creature creature)
    {
        if (creature != base.Owner)
        {
            return;
        }

        if (base.Owner.PetOwner is not Player { Creature: { IsAlive: true } queen })
        {
            return;
        }

        decimal halfMaxHp = decimal.Floor(base.Owner.MaxHp / 2m);
        if (halfMaxHp < 1m)
        {
            return;
        }
        await PowerCmd.Apply<NextTurnAmalgamSummonPendingPower>(queen, halfMaxHp, queen, null);
    }
}

