using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>特殊意图行动：展示 unknown 意图，并执行自定义逻辑。</summary>
public sealed class AmalgamSpecialIntentAction : AmalgamActionModel
{
    private readonly string _moveId;
    private readonly string _intentDescriptionKey;
    private readonly Func<PlayerChoiceContext, Creature, Creature, Task> _execute;

    public AmalgamSpecialIntentAction(
        string moveId,
        string intentDescriptionKey,
        Func<PlayerChoiceContext, Creature, Creature, Task> execute)
    {
        _moveId = moveId;
        _intentDescriptionKey = intentDescriptionKey;
        _execute = execute;
    }

    public static readonly float CastAnimDelay = 1.5f;

    protected override MoveState CreateMoveState() =>
        new(
            _moveId,
            _ => Task.CompletedTask,
            new AmalgamSpecialIntent(_intentDescriptionKey));

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        if (amalgam.PetOwner is not { Creature: { } owner } || !owner.IsAlive)
        {
            return;
        }

        await _execute(choiceContext, amalgam, owner);
    }
}
