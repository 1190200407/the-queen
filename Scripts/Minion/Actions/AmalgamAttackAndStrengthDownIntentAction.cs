using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

public sealed class AmalgamAttackAndStrengthDownIntentAction : AmalgamActionModel
{
    public override string Key => "attack_and_strength_down";

    private readonly decimal _strengthLoss;

    public AmalgamAttackAndStrengthDownIntentAction(decimal damage, decimal strengthLoss)
    {
        Amount = damage;
        _strengthLoss = strengthLoss;
    }

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_ATTACK_AND_STRENGTH_DOWN",
            _ => Task.CompletedTask,
            new AmalgamSingleAttackIntent(Amount),
            new AmalgamStrengthDownIntent(_strengthLoss));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        if (Amount > 0m)
        {
            await AmalgamActionRegistry.ExecuteTemporaryAsync(
                choiceContext,
                amalgam,
                AmalgamActionRegistry.CreateOffense(Amount));
        }

        if (_strengthLoss > 0m)
        {
            await AmalgamActionRegistry.ExecuteTemporaryAsync(
                choiceContext,
                amalgam,
                AmalgamActionRegistry.Rent<AmalgamStrengthDownIntentAction>(_strengthLoss));
        }
    }
}
