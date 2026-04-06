using MegaCrit.Sts2.Core.Entities.Cards;

namespace ComicChess.TheQueen;

/// <summary>
/// 引擎 <see cref="CardTag"/> 未开放扩展时，使用未占用的枚举底层值作为 mod 专用标签（参考完美打击对 <see cref="CardTag.Strike"/> 的用法）。
/// </summary>
public static class QueenCardTags
{
	/// <summary>抓挠体系攻击牌；与 <see cref="ScratchTaggedCard"/> 配合。</summary>
	public const CardTag Scratch = (CardTag)79;
}
