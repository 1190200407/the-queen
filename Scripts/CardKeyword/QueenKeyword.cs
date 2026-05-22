using STS2RitsuLib.Content;

namespace ComicChess.TheQueen;

public static class QueenKeyword
{
    public static readonly string Fade = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Fade));
    public static readonly string AmalgamComposite = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(AmalgamComposite));
}
