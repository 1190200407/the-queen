using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：回合末每执行一次为女王叠 <strong>1</strong> 层 <see cref="EmptyCupPendingPower"/>。</summary>
public sealed class AmalgamEmptyCupIntentAction : AmalgamActionModel
{
    public AmalgamEmptyCupIntentAction()
        : base()
    {
    }

    public static readonly float CastAnimDelay = 1.5f;

    public override LocString IntentTitle => new("monsters", "FRIENDLY_AMALGAM.intent_empty_cup.title");

    public override LocString GetIntentDescription() =>
        new("monsters", "FRIENDLY_AMALGAM.intent_empty_cup.description");

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_ENERGY_DRAW",
            _ => Task.CompletedTask,
            new AmalgamEnergyIntent(),
            new AmalgamDrawIntent());
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        _ = choiceContext;
        if (amalgam.PetOwner is not { Creature: { } owner } || !owner.IsAlive)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Cast", CastAnimDelay);
        await PowerCmd.Apply<EmptyCupPendingPower>(owner, 1m, amalgam, null);
    }
}
