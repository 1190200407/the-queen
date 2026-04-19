using MegaCrit.Sts2.Core.Entities.Powers;

namespace ComicChess.TheQueen;

/// <summary>万灵破军：聚合体进攻意图对每名存活敌人各结算一次。</summary>
public sealed class AmalgamArmyBreakPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
}
