using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>
/// 覆盖聚合体展示与部分回合末逻辑的强制行动模型。
/// </summary>
public abstract class AmalgamForcedActionModel : AmalgamActionModel
{
    protected AmalgamForcedActionModel(decimal amount) : base(amount)
    {
    }

    /// <summary>玩家侧回合结束时是否跳过灯槽 <see cref="AmalgamActionModel"/> 的执行（仍刷新展示）。</summary>
    public abstract bool SkipsPlayerTurnEndTorchExecution { get; }

    public virtual bool IsSleepingAction => false;

    public virtual bool ClearAfterExecute { get; } = true;

	public virtual Task OnBeginAsync(Creature creature) => Task.CompletedTask;
}

/// <summary>紧急避险：强制沉睡展示并跳过本回合末灯槽执行。</summary>
public sealed class AmalgamEmergencySleepForcedActionModel : AmalgamForcedActionModel
{
    public AmalgamEmergencySleepForcedActionModel(decimal amount) : base(amount)
    {
    }

    protected override MoveState CreateMoveState() => FriendlyAmalgam.SleepOverlayMoveState;

	public override LocString IntentTitle => new("intents", "AMALGAM_SLEEP.title");
    public override LocString GetIntentDescription()
    {
		return new("intents", "AMALGAM_SLEEP.description");
    }

    public override bool ClearAfterExecute => false;

    protected override Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        return Task.CompletedTask;
    }

    public override bool SkipsPlayerTurnEndTorchExecution => true;

    public override bool IsSleepingAction => true;
}
