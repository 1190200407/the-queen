using MegaCrit.Sts2.Core.Entities.Powers;

namespace ComicChess.TheQueen;

/// <summary>可叠加：每层使玩家回合结束时聚合体多执行一轮灯槽意图（在基础 1 次之上再加 <see cref="PowerModel.Amount"/> 次）。</summary>
public sealed class TerminusFormPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldPlayVfx => false;
}
