using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 埋地（聚合体版）：回合开始时格挡不移除；格挡被打破时移除自身并使聚合体沉睡 1 回合。
/// （格挡保留与打破钩子对齐原版 <see cref="MegaCrit.Sts2.Core.Models.Powers.BurrowedPower"/>，效果按 mod 沉睡体系改写。）
/// </summary>
public sealed class AmalgamBurrowedPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomPackedIconPath => "res://images/atlases/power_atlas.sprites/burrowed_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/burrowed_power.png";

    public override bool ShouldClearBlock(Creature creature)
    {
        if (base.Owner != creature)
        {
            return true;
        }

        return false;
    }

    public override async Task AfterBlockBroken(Creature creature)
    {
        if (creature != base.Owner || base.Owner.Monster is not FriendlyAmalgam)
        {
            return;
        }

        await PowerCmd.Remove(this);
        await PowerCmd.Apply<AmalgamSleepPower>(
            base.Owner,
            2m,
            applier: base.Owner.PetOwner?.Creature,
            cardSource: null);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await CreatureCmd.LoseBlock(oldOwner, 999m);
    }
}
