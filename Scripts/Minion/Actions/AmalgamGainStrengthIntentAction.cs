using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：聚合体自身获得 <see cref="AmalgamActionModel.Amount"/> 点力量。</summary>
public sealed class AmalgamGainStrengthIntentAction : AmalgamActionModel
{
    public AmalgamGainStrengthIntentAction(decimal strength) : base(strength)
    {
    }

    public static readonly float CastAnimDelay = 1.5f;

    public override LocString IntentTitle => new("monsters", "FRIENDLY_AMALGAM.intent_strength.title");

    public override LocString GetIntentDescription()
    {
        LocString desc = new("monsters", "FRIENDLY_AMALGAM.intent_strength.description");
        desc.Add("Amount", Amount);
        return desc;
    }

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
