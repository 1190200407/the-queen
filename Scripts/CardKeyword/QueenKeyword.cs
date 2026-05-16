using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

[RegisterOwnedCardKeyword(nameof(Fade), IconPath = "res://icon.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(AmalgamComposite), IconPath = "res://icon.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.AfterCardDescription)]
public class QueenKeyword
{
    public static readonly string Fade = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Fade));
    public static readonly string AmalgamComposite = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(AmalgamComposite));
}
