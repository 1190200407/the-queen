using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

/// <summary>本 mod 已注册关键词的 <see cref="CardKeyword"/> 缓存，避免重复解析 id。</summary>
internal static class QueenModKeywords
{
	internal static readonly CardKeyword Fade = ModKeywordRegistry.GetCardKeyword(QueenKeyword.Fade);
}
