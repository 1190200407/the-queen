using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>
/// 聚合体专用意图基类（独立模型，不走 Power/Action 系统）。
/// </summary>
public abstract class AmalgamActionModel
{
    private MoveState? _moveState;

    protected AmalgamActionModel(decimal amount)
    {
        Amount = amount;
    }

    public decimal Amount { get; }

    public abstract LocString IntentTitle { get; }

    public abstract LocString GetIntentDescription();

    public virtual string? IntentIconPath => null;

    public MoveState MoveState => _moveState ??= CreateMoveState();

    /// <summary>意图条等展示用；进攻类可在此把 <see cref="Hook.ModifyDamage"/>（出手方为聚合体）与卡面数字对齐。默认等同 <see cref="MoveState"/>。</summary>
    public virtual MoveState GetMoveStateForDisplay(Creature amalgam) => MoveState;

    public Task ExecuteAsync(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        if (!amalgam.IsAlive || amalgam.CombatState == null)
        {
            return Task.CompletedTask;
        }

        return OnExecute(choiceContext, amalgam);
    }

    protected abstract MoveState CreateMoveState();

    protected abstract Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam);
}