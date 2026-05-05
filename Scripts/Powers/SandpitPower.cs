using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

/// <summary>
/// 沙坑（女王版）：倒计时归零时，使所有敌人直接死亡。
/// </summary>
public sealed class SandpitPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 复用原版 Sandpit 的图标资源。
    public override string? CustomPackedIconPath => "res://images/atlases/power_atlas.sprites/sandpit_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/sandpit_power.png";

    // public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    // {
    //     _ = combatState;
    //     if (side == CombatSide.Enemy)
    //     {
    //         await PowerCmd.ModifyAmount(this, 1m, base.Owner, null);
    //     }
    // }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        _ = oldOwner;
        if (base.Owner?.CombatState is not { } combatState)
        {
            return;
        }

        if (base.Owner.Player is { } player)
        {
            await CreatureCmd.TriggerAnim(base.Owner, "Attack", player.Character.AttackAnimDelay);
        }
        IReadOnlyList<Creature> allAffectedCreature = combatState.Enemies.Where(static c => c.IsAlive).ToArray();
        foreach (Creature enemy in allAffectedCreature)
        {
            await CreatureCmd.Kill(enemy, force: true);
        }
    }
}

