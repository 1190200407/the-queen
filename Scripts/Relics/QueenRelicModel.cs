
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ComicChess.TheQueen;

[RegisterRelic(typeof(QueenRelicPool), Inherit = true)]
public abstract class QueenRelicModel : ModRelicTemplate
{
    public override string? CustomIconPath =>
        $"res://TheQueen/images/relics/{ResolveRelicIconKey()}.png";

    public override string? CustomIconOutlinePath =>
        $"res://TheQueen/images/relics/{ResolveRelicIconKey()}_outline.png";

    public override string? CustomBigIconPath =>
        $"res://TheQueen/images/relics/big/{ResolveRelicIconKey()}.png";

    private string ResolveRelicIconKey()
    {
        string key = Id.Entry.ToLowerInvariant().Replace("the_queen_relic_", "");
        if (key.EndsWith("_relic"))
        {
            key = key[..^"_relic".Length];
        }

        return key;
    }
}
