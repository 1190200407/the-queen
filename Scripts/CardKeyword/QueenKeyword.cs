using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace ComicChess.TheQueen;

public class QueenKeyword
{
    [CustomEnum("FADE")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword fade;

    [CustomEnum("BINDING_OATH")]
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword bindingOath;

    [CustomEnum("SOUL_LAMP")]
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword soulLamp;

    [CustomEnum("AMALGAM_COMPOSITE")]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword amalgamComposite;
}

