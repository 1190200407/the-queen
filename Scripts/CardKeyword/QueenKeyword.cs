using STS2RitsuLib.Content;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

public static class QueenKeyword
{
    public const string CompositeKeywordIconPath = "res://TheQueen/images/charui/queen_boss.png";

    public static readonly string Fade = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Fade));

    public static readonly string AmalgamComposite =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(AmalgamComposite));

    public static IEnumerable<AmalgamCompositeKey> EnumerateCompositeKeys()
    {
        foreach (AmalgamCompositeKey key in Enum.GetValues<AmalgamCompositeKey>())
        {
            if (key != AmalgamCompositeKey.None)
            {
                yield return key;
            }
        }
    }

    public static string GetAmalgamCompositeKeywordStem(AmalgamCompositeKey key)
    {
        if (key == AmalgamCompositeKey.None)
        {
            throw new ArgumentOutOfRangeException(nameof(key), key, "None is not a registered amalgam composite keyword.");
        }

        return $"AmalgamComposite{(int)key}";
    }

    public static string GetAmalgamCompositeKeywordId(AmalgamCompositeKey key) =>
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, GetAmalgamCompositeKeywordStem(key));

    public static string GetCompositeIntentTitleLocKey(AmalgamCompositeKey key) =>
        $"COMPOSITE_INTENT_TITLE_{key}";

    public static void RegisterCompositeKeywords(
        ModKeywordRegistry registry,
        string? iconPath = CompositeKeywordIconPath,
        ModKeywordCardDescriptionPlacement cardDescriptionPlacement = ModKeywordCardDescriptionPlacement.AfterCardDescription,
        bool includeInCardHoverTip = true)
    {
        foreach (AmalgamCompositeKey key in EnumerateCompositeKeys())
        {
            registry.RegisterCardKeywordOwnedByLocNamespace(
                GetAmalgamCompositeKeywordStem(key),
                iconPath,
                cardDescriptionPlacement,
                includeInCardHoverTip);
        }
    }
}
