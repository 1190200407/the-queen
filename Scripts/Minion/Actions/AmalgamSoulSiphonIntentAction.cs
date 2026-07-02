using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>??????? action??? intent????????????????????</summary>
public sealed class AmalgamSoulSiphonIntentAction : AmalgamActionModel
{
    public override string Key => "soul_siphon";

    private readonly decimal _stacks;

    public AmalgamSoulSiphonIntentAction(decimal stacks)
        : base(stacks)
    {
        _stacks = stacks;
    }

    public static readonly float CastAnimDelay = 1.5f;

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_SOUL_SIPHON",
            _ => Task.CompletedTask,
            new AmalgamLoseStrengthAllIntent(_stacks),
            new AmalgamGrantStrengthIntent(_stacks),
            new AmalgamGrantDexterityIntent(_stacks));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        ICombatState? combatState = amalgam.CombatState;
        if (combatState == null || amalgam.PetOwner is not Player queen || !queen.Creature.IsAlive || _stacks <= 0m)
        {
            return;
        }

        Creature[] alive = combatState.Enemies.Where(e => e.IsAlive).ToArray();
        if (alive.Length == 0)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Cast", CastAnimDelay);

        // ???????????? Strength?
        foreach (Creature enemy in alive)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, -_stacks, queen.Creature, null);
        }

        await PowerCmd.Apply<StrengthPower>(choiceContext, queen.Creature, _stacks, queen.Creature, null);
        await PowerCmd.Apply<DexterityPower>(choiceContext, queen.Creature, _stacks, queen.Creature, null);
    }
}

