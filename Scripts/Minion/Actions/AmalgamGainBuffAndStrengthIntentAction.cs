using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：同一个行动占位，展示为「获得某 Buff + 获得力量」�?/summary>
public sealed class AmalgamGainBuffAndStrengthIntentAction<TPower> : AmalgamActionModel
    where TPower : PowerModel
{
    public override string Key => GenericPoolKey("gain_buff_and_strength", typeof(TPower));

    private readonly string _buffEntryId;
    private readonly decimal _strength;

    public AmalgamGainBuffAndStrengthIntentAction(decimal buffStacks, string buffEntryId, decimal strengthStacks)
    {
        Amount = buffStacks;
        _buffEntryId = buffEntryId;
        _strength = strengthStacks;
    }

    public static readonly float CastAnimDelay = 1.5f;

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_GAIN_BUFF_AND_STRENGTH",
            _ => Task.CompletedTask,
            new AmalgamGainBuffIntent(_buffEntryId, Amount),
            new AmalgamGainStrengthIntent(_strength));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        _ = choiceContext;
        if (amalgam.PetOwner is not { Creature: { } owner } || !owner.IsAlive)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Buff", CastAnimDelay);

        if (Amount > 0m)
        {
            await PowerCmd.Apply<TPower>(choiceContext, amalgam, Amount, owner, null);
        }

        if (_strength > 0m)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, amalgam, _strength, owner, null);
        }
    }
}

