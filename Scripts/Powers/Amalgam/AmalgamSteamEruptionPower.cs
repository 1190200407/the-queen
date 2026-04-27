using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Logging;

namespace ComicChess.TheQueen;

/// <summary>蒸汽喷发：聚合体每行动一次，获得更多层数。</summary>
public sealed class AmalgamSteamEruptionPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 复用原版 Steam Eruption 的图标资源。
    public override string? CustomPackedIconPath => "res://images/atlases/power_atlas.sprites/steam_eruption_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/steam_eruption_power.png";

    internal async Task OnAmalgamActAsync(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        _ = choiceContext;
        if (base.CombatState == null || amalgam != base.Owner || !amalgam.IsAlive)
        {
            return;
        }

        const decimal gainPerAct = 3m;
        Flash();
        await PowerCmd.Apply<AmalgamSteamEruptionPower>(amalgam, gainPerAct, applier: amalgam.PetOwner?.Creature, cardSource: null, silent: true);
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != base.Owner || Amount <= 0m)
        {
            return;
        }

        CombatState? combatState = creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        Creature[] alive = combatState.Enemies.Where(e => e.IsAlive).ToArray();
        if (alive.Length == 0)
        {
            return;
        }

        if (base.Owner.PetOwner?.Creature is { } dealer)
        {
            await CreatureCmd.Damage(choiceContext, alive, Amount, ValueProp.Unpowered, dealer, null);
        }
    }
}


