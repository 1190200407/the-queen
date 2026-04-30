using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

public interface IAmalgamEventListener
{
    Task OnAmalgamDamagedCreatureAsync(
        CombatState combatState,
        PlayerChoiceContext choiceContext,
        Creature amalgam,
        Creature damagedEnemy,
        IEnumerable<DamageResult> damageResults)
    {
        return Task.CompletedTask;
    }

    Task OnAmalgamActAsync(CombatState combatState, PlayerChoiceContext choiceContext, Creature amalgam)
    {
        return Task.CompletedTask;
    }

    Task OnAmalgamWakeFromSleepAsync(CombatState combatState, Creature amalgam)
    {
        return Task.CompletedTask;
    }

    Task OnAmalgamEscapeAsync(CombatState combatState, Creature amalgam)
    {
        return Task.CompletedTask;
    }
}