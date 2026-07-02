using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>通用聚合�?Buff 意图：聚合体自身获得若干层某�?Buff（Power）�?/summary>
public sealed class AmalgamGainBuffIntentAction<T> : AmalgamActionModel
    where T : PowerModel
{
    public override string Key => GenericPoolKey("gain_buff", typeof(T));

    private string _buffEntryId = string.Empty;

    public AmalgamGainBuffIntentAction()
    {
    }

    public AmalgamGainBuffIntentAction(decimal stacks, string buffEntryId)
        : base(stacks)
    {
        _buffEntryId = buffEntryId;
    }

    public override bool Init(decimal amount) => false;

    public override bool Init(object[] args)
    {
        if (!AmalgamActionArgs.TryGetDecimal(args, 0, out decimal stacks) || !AmalgamActionArgs.IsPositive(stacks))
        {
            return false;
        }

        string? buffEntryId = AmalgamActionArgs.TryGetString(args, 1);
        if (buffEntryId == null)
        {
            return false;
        }

        ResetForInit();
        Amount = stacks;
        _buffEntryId = buffEntryId;
        return true;
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
        await PowerCmd.Apply<T>(amalgam, Amount, owner, null);
    }
}

