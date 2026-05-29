using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>蒸汽喷发：聚合体每行动一次获得更多层数；沉睡时按层数对全体敌人造成伤害。</summary>
public sealed class AmalgamSteamEruptionPower : QueenPowerModel, IAmalgamEventListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/steam_eruption_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/steam_eruption_power.png";

    public async Task OnAmalgamActAsync(ICombatState combatState, PlayerChoiceContext choiceContext, Creature amalgam)
    {
        _ = combatState;
        if (amalgam != base.Owner || !amalgam.IsAlive)
        {
            return;
        }

        const decimal gainPerAct = 3m;
        Flash();
        await PowerCmd.Apply<AmalgamSteamEruptionPower>(
            choiceContext,
            amalgam,
            gainPerAct,
            applier: amalgam.PetOwner?.Creature,
            cardSource: null,
            silent: true);
    }

    public async Task OnAmalgamFallAsleepAsync(ICombatState combatState, Creature amalgam)
    {
        if (amalgam != base.Owner || Amount <= 0m)
        {
            return;
        }

        //TODO 找个更合适的特效
        VfxCmd.PlayVfx(base.Owner.GetCreatureNode().VfxSpawnPosition, "vfx/vfx_scream", base.Owner.GetVfxContainer());
        SfxCmd.Play("event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_die");

        Creature[] alive = combatState.Enemies.Where(e => e.IsAlive).ToArray();
        if (alive.Length == 0)
        {
            return;
        }

        if (base.Owner.PetOwner?.Creature is not { } dealer)
        {
            return;
        }

        Flash();
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            alive,
            Amount,
            ValueProp.Unpowered,
            dealer,
            null);
    }
}
