
using Godot;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ComicChess.TheQueen;

[RegisterRelic(typeof(QueenRelicPool), Inherit = true)]
public abstract class QueenRelicModel : ModRelicTemplate
{
    // // 小图标
    // public override string PackedIconPath => $"res://TheQueen/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    // // 轮廓图标
    // protected override string PackedIconOutlinePath => $"res://TheQueen/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    // // 大图标
    // protected override string BigIconPath => $"res://TheQueen/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    // 小图标
    public override string PackedIconPath
    {
        get
        {
            string key = Id.Entry.ToLowerInvariant().Replace("sts2_comicchess_thequeen_relic_", "");
            string custom = $"res://TheQueen/images/relics/{key}.png";
            return ResourceLoader.Exists(custom) ? custom : "res://TheQueen/images/relics/relic.png";
        }
    }
    // 轮廓图标
    protected override string PackedIconOutlinePath
    {
        get
        {
            string key = Id.Entry.ToLowerInvariant().Replace("sts2_comicchess_thequeen_relic_", "");
            string custom = $"res://TheQueen/images/relics/{key}_outline.png";
            return ResourceLoader.Exists(custom) ? custom : "res://TheQueen/images/relics/relic_outline.png";
        }
    }
    // 大图标
    protected override string BigIconPath
    {
        get
        {
            string key = Id.Entry.ToLowerInvariant().Replace("sts2_comicchess_thequeen_relic_", "");
            string custom = $"res://TheQueen/images/relics/big/{key}.png";
            return ResourceLoader.Exists(custom) ? custom : "res://TheQueen/images/relics/big/relic.png";
        }
    }
}