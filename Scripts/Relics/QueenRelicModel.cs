
using Godot;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ComicChess.TheQueen;

[RegisterRelic(typeof(QueenRelicPool), Inherit = true)]
public abstract class QueenRelicModel : ModRelicTemplate
{
    public override string? CustomIconPath
    {
        get
        {
            string custom = $"res://TheQueen/images/relics/{ResolveRelicIconKey()}.png";
            return ResourceLoader.Exists(custom) ? custom : "res://TheQueen/images/relics/relic.png";
        }
    }

    public override string? CustomIconOutlinePath
    {
        get
        {
            string custom = $"res://TheQueen/images/relics/{ResolveRelicIconKey()}_outline.png";
            return ResourceLoader.Exists(custom) ? custom : "res://TheQueen/images/relics/relic_outline.png";
        }
    }

    public override string? CustomBigIconPath
    {
        get
        {
            string custom = $"res://TheQueen/images/relics/big/{ResolveRelicIconKey()}.png";
            return ResourceLoader.Exists(custom) ? custom : "res://TheQueen/images/relics/big/relic.png";
        }
    }

    private string ResolveRelicIconKey()
    {
        string key = Id.Entry.ToLowerInvariant().Replace("sts2_comicchess_thequeen_relic_", "");
        if (key.EndsWith("_relic"))
        {
            key = key[..^"_relic".Length];
        }

        return key;
    }
}