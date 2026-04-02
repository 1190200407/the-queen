using BaseLib.Abstracts;

namespace ComicChess.TheQueen;

public abstract class QueenRelicModel : CustomRelicModel
{
    // // 小图标
    // public override string PackedIconPath => $"res://TheQueen/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    // // 轮廓图标
    // protected override string PackedIconOutlinePath => $"res://TheQueen/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    // // 大图标
    // protected override string BigIconPath => $"res://TheQueen/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    // 小图标
    public override string PackedIconPath => $"res://TheQueen/images/relics/relic.png";
    // 轮廓图标
    protected override string PackedIconOutlinePath => $"res://TheQueen/images/relics/relic_outline.png";
    // 大图标
    protected override string BigIconPath => $"res://TheQueen/images/relics/big/relic.png";
}