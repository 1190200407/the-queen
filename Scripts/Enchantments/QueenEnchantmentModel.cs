using BaseLib.Abstracts;

namespace ComicChess.TheQueen;

public abstract class QueenEnchantmentModel : CustomEnchantmentModel
{
    protected override string? CustomIconPath => $"res://TheQueen/images/enchantments/{Id.Entry.ToLowerInvariant().Replace("comicchess-", "")}.png";
}