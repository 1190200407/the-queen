using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：聚合体自身获�?<see cref="AmalgamActionModel.Amount"/> 点力量�?/summary>
public sealed class AmalgamGainStrengthIntentAction : AmalgamSingleDecimalActionModel
{
    public override string Key => "strength";

    public AmalgamGainStrengthIntentAction()
    {
    }

    public AmalgamGainStrengthIntentAction(decimal strength)
        : base(strength)
    {
    }

    public override bool Init(decimal amount) => TryInitSingleDecimal(amount);

    public override bool Init(object[] args) => TryInitSingleDecimal(args);

    public static readonly float CastAnimDelay = 1.5f;

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_STRENGTH",
            _ => Task.CompletedTask,
            new AmalgamGainStrengthIntent(Amount));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        _ = choiceContext;
        if (amalgam.PetOwner is not { Creature: { } owner } || !owner.IsAlive || Amount <= 0m)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Buff", CastAnimDelay);
        await PowerCmd.Apply<StrengthPower>(amalgam, Amount, owner, null);
    }
}
