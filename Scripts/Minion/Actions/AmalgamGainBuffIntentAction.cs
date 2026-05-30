using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>通用聚合体 Buff 意图：聚合体自身获得若干层某种 Buff（Power）。</summary>
public sealed class AmalgamGainBuffIntentAction<T> : AmalgamActionModel
    where T : PowerModel
{
    private readonly string _buffEntryId;

    public AmalgamGainBuffIntentAction(decimal stacks, string buffEntryId)
        : base(stacks)
    {
        _buffEntryId = buffEntryId;
    }

    public static readonly float CastAnimDelay = 1.5f;

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_GAIN_BUFF",
            _ => Task.CompletedTask,
            new AmalgamGainBuffIntent(_buffEntryId, Amount));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        _ = choiceContext;
        if (amalgam.PetOwner is not { Creature: { } owner } || !owner.IsAlive || Amount <= 0m)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Buff", CastAnimDelay);
        await PowerCmd.Apply<T>(choiceContext, amalgam, Amount, owner, null);
    }
}

