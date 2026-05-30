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

/// <summary>疼痛戳刺（聚合体版）：聚合体造成未被格挡的伤害时，你抽牌。</summary>
public sealed class AmalgamPainfulStabsPower : QueenPowerModel, IAmalgamEventListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/painful_stabs_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/painful_stabs_power.png";

    public async Task OnAmalgamDamagedCreatureAsync(
        CombatState combatState,
        PlayerChoiceContext choiceContext,
        Creature amalgam,
        Creature damagedEnemy,
        IEnumerable<DamageResult> damageResults)
    {
        _ = damagedEnemy;

        if (amalgam != base.Owner || Amount <= 0m)
        {
            return;
        }

        if (base.Owner.PetOwner is not { Creature: { IsAlive: true } } player)
        {
            return;
        }

        int triggers = damageResults.Count(static r => r.UnblockedDamage > 0);
        if (triggers <= 0)
        {
            return;
        }

        Flash();
        await CardPileCmd.Draw(choiceContext, (int)(Amount * triggers), player);
    }
}

