using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MinionLib.Commands;
using MinionLib.Models;

namespace ComicChess.TheQueen;

public static class FriendlyAmalgamCmd
{
    public static async Task Summon(PlayerChoiceContext choiceContext, Player owner, decimal amount, AbstractModel? source)
    {
        CombatState? combatState = owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        amount = Hook.ModifySummonAmount(combatState, owner, amount, source);
        if (amount <= 0m)
        {
            return;
        }

        Creature? existing = combatState.Allies.FirstOrDefault(c =>
            c.PetOwner == owner && c.Monster is FriendlyAmalgam);

        if (existing is { IsAlive: true })
        {
            await CreatureCmd.GainMaxHp(existing, amount);
            return;
        }

        bool isReviving = existing != null;
        Creature minion = existing ?? await MinionCmd.AddMinion<FriendlyAmalgam>(
            owner,
            new MinionSummonOptions(Position: MinionPosition.Front));

        if (isReviving)
        {
            owner.PlayerCombatState?.AddPetInternal(minion);
        }

        await CreatureCmd.SetMaxHp(minion, amount);
        await CreatureCmd.Heal(minion, amount, isReviving);
        CombatManager.Instance.History.Summoned(combatState, (int)amount, owner);
        await Hook.AfterSummon(combatState, choiceContext, owner, amount);
    }
}
