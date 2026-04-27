using MegaCrit.Sts2.Core.Entities.Powers;

namespace ComicChess.TheQueen;

/// <summary>精心挑选：标记聚合体优先攻击的敌人；全场至多一层（由出牌逻辑保证）。</summary>
public sealed class AmalgamPickLockPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;
}

