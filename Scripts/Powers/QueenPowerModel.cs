using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace ComicChess.TheQueen;

public abstract class QueenPowerModel : CustomPowerModel
{
    // 自定义图标路径
    // public override string? CustomPackedIconPath => $"res://TheQueen/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    // public override string? CustomBigIconPath => $"res://TheQueen/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomPackedIconPath => $"res://TheQueen/images/powers/power.png";
    public override string? CustomBigIconPath => $"res://TheQueen/images/powers/big/power.png";
}