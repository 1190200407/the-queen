using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>失衡（聚合体版）：对齐原版 <c>ImbalancedPower</c>；攻击被完全格挡时陷入沉睡。</summary>
public sealed class AmalgamImbalancedPower : QueenPowerModel, IAmalgamEventListener
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/imbalanced_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/imbalanced_power.png";

    public async Task OnAmalgamDamagedCreatureAsync(
        CombatState combatState,
        PlayerChoiceContext choiceContext,
        Creature amalgam,
        Creature damagedEnemy,
        IEnumerable<DamageResult> damageResults)
    {
        _ = combatState;
        _ = choiceContext;
        _ = damagedEnemy;

        if (amalgam != base.Owner || !base.Owner.IsAlive)
        {
            return;
        }

        if (!damageResults.Any(static r => r.WasFullyBlocked))
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<AmalgamSleepPower>(choiceContext, base.Owner, 1m, base.Owner, null);
    }
}
