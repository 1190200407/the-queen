using STS2RitsuLib.Scaffolding.Content;
namespace ComicChess.TheQueen;

public abstract class QueenEnchantmentModel : ModEnchantmentTemplate
{
    public override string? CustomIconPath => $"res://TheQueen/images/enchantments/{Id.Entry.ToLowerInvariant().Replace("sts2_comicchess_thequeen_enchantment_", "")}.png";
}