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
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 复用原版 Ravenous 的图标资源。
    public override string? CustomPackedIconPath => "res://images/atlases/power_atlas.sprites/ravenous_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/ravenous_power.png";

    private bool isAsleep = false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
    {
        _ = deathAnimLength;
        if (base.Owner == null)
        {
            return;
        }
        if (wasRemovalPrevented || !base.Owner.IsAlive || target == base.Owner)
        {
            return;
        }

        // 任意单位死亡都触发（不区分阵营）。
        if (Amount <= 0m)
        {
            return;
        }

        Flash();
        SfxCmd.Play("event:/sfx/enemy/enemy_attacks/corpse_slugs/corpse_slugs_ravenous");

        // 本回合沉睡：强制沉睡展示并跳过本回合末灯槽执行；下回合开始时清除强制行动。
        FriendlyAmalgam? amalgamModel = base.Owner.Monster as FriendlyAmalgam;
        if (amalgamModel != null)
        {
            await amalgamModel.FallAsleep(FriendlyAmalgam.SleepReason.Ravenous);
            isAsleep = true;
            await PowerCmd.Apply<StrengthPower>(base.Owner, Amount, base.Owner, null);
        }
    }

    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _ = choiceContext;

        CombatState? combatState = base.Owner.CombatState;
        if (combatState != null && isAsleep)
        {
            if (base.Owner.Monster is FriendlyAmalgam amalgam)
            {
                await amalgam.WakeUp(FriendlyAmalgam.SleepReason.Ravenous);
                isAsleep = false;
            }
        }
    }
}

