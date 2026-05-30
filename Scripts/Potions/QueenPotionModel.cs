
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ComicChess.TheQueen;

[RegisterPotion(typeof(QueenPotionPool), Inherit = true)]
public abstract class QueenPotionModel : ModPotionTemplate
{
    public override string? CustomImagePath =>
        $"res://TheQueen/images/potions/{ResolvePotionAssetKey()}.png";

    public override string? CustomOutlinePath =>
        $"res://TheQueen/images/potions/{ResolvePotionAssetKey()}_outline.png";

    private string ResolvePotionAssetKey() =>
        Id.Entry.ToLowerInvariant().Replace("the_queen_potion_", "");
}
