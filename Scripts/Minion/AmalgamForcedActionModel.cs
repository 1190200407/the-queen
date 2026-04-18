using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>
/// 覆盖聚合体展示与部分回合末逻辑的强制行动模型（与灯槽内的 <see cref="AmalgamActionModel"/> 并存）。
/// </summary>
public abstract class AmalgamForcedActionModel
{
	/// <summary>意图条 / 怪物 MoveState 展示用覆盖。</summary>
	public abstract MoveState GetOverlayMoveState(Creature amalgamCreature);

	/// <summary>玩家侧回合结束时是否跳过灯槽 <see cref="AmalgamActionModel"/> 的执行（仍刷新展示）。</summary>
	public abstract bool SkipsPlayerTurnEndTorchExecution { get; }

	/// <summary>是否视为沉睡而不替主人承伤。</summary>
	public abstract bool IsSleepingForBodyguard { get; }

	public virtual Task OnBeginAsync(Creature creature) => Task.CompletedTask;
}

/// <summary>紧急避险：强制沉睡展示并跳过本回合末灯槽执行。</summary>
public sealed class AmalgamEmergencySleepForcedActionModel : AmalgamForcedActionModel
{
	public override MoveState GetOverlayMoveState(Creature _) => FriendlyAmalgam.SleepOverlayMoveState;

	public override bool SkipsPlayerTurnEndTorchExecution => true;

	public override bool IsSleepingForBodyguard => true;
}
