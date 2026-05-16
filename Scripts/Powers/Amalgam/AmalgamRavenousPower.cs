using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Combat;

namespace ComicChess.TheQueen;

/// <summary>
/// 饥饿（噬尸）：任意单位死亡时，聚合体本回合进入沉睡（跳过回合末灯槽执行）并获得等同于层数的力量。
/// </summary>
public sealed class AmalgamRavenousPower : QueenPowerModel
{
    private sealed class Data
    {
        public bool ShouldSleep = false;
    }

    protected override object? InitInternalData()
    {
        return new Data();
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 复用原版 Ravenous 的图标资源。
    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/ravenous_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/ravenous_power.png";

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
    {
        _ = deathAnimLength;
        if (base.Owner == null)
        {
            return Task.CompletedTask;
        }
        if (wasRemovalPrevented || !base.Owner.IsAlive || target == base.Owner)
        {
            return Task.CompletedTask;
        }

        // 任意单位死亡都触发（不区分阵营）。
        if (Amount <= 0m)
        {
            return Task.CompletedTask;
        }

        Flash();
        GetInternalData<Data>().ShouldSleep = true;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (!GetInternalData<Data>().ShouldSleep)
        {
            return;
        }

        GetInternalData<Data>().ShouldSleep = false;
        SfxCmd.Play("event:/sfx/enemy/enemy_attacks/corpse_slugs/corpse_slugs_ravenous");
        // 本回合沉睡：强制沉睡展示并跳过本回合末灯槽执行；下回合开始时清除强制行动。
        FriendlyAmalgam? amalgamModel = base.Owner.Monster as FriendlyAmalgam;
        if (amalgamModel != null)
        {
            await PowerCmd.Apply<AmalgamSleepPower>(base.Owner, 1m, applier: base.Owner.PetOwner?.Creature, cardSource: null);
            await PowerCmd.Apply<StrengthPower>(base.Owner, Amount, base.Owner, null);
        }
    }
}

