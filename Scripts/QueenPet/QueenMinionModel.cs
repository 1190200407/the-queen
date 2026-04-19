using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>单仆从用：替代 MinionLib 的 <c>MinionModel</c>。</summary>
public abstract class QueenMinionModel : MonsterModel
{
    public override string DeathSfx => "event:/sfx/characters/osty/osty_die";

    public override bool HasDeathSfx => true;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState idle = new("MINION_IDLE", _ => Task.CompletedTask) { FollowUpState = null };
        idle.FollowUpState = idle;
        return new MonsterMoveStateMachine([idle], idle);
    }

    public virtual Task OnSummon(Player owner, Creature self, MinionSummonOptions options) =>
        Task.CompletedTask;
}

/// <summary>召唤聚合体时可选上下文（例如来自哪张牌）。</summary>
public readonly record struct MinionSummonOptions(CardModel? Source = null);
