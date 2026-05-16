using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>
/// Mod 专用卡牌标签（经 <see cref="RegisteredCardTagIds"/> 写入牌实例，用 <see cref="STS2RitsuLib.CardTags.ModCardTagExtensions.HasModCardTag"/> 判定）。
/// 原版 <see cref="MegaCrit.Sts2.Core.Entities.Cards.CardTag"/> 如 Strike、Defend 仍通过 <see cref="MegaCrit.Sts2.Core.Models.CardModel.CanonicalTags"/> 声明。
/// </summary>
[RegisterOwnedCardTag(nameof(Scratch))]
[RegisterOwnedCardTag(nameof(LearnIntent))]
public static class QueenCardTags
{
	public static readonly string Scratch = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(Scratch));

	public static readonly string LearnIntent = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(LearnIntent));
}
