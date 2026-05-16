
using Godot;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ComicChess.TheQueen;

[RegisterPotion(typeof(QueenPotionPool), Inherit = true)]
public abstract class QueenPotionModel : ModPotionTemplate
{
    public override string? CustomImagePath
    {
        get
        {
            string key = Id.Entry.ToLowerInvariant().Replace("sts2_comicchess_thequeen_potion_", "");
            string custom = $"res://TheQueen/images/potions/{key}.png";
            return ResourceLoader.Exists(custom) ? custom : "res://TheQueen/images/potions/potion.png";
        }
    }
    public override string? CustomOutlinePath
    {
        get
        {
            string key = Id.Entry.ToLowerInvariant().Replace("sts2_comicchess_thequeen_potion_", "");
            string custom = $"res://TheQueen/images/potions/{key}_outline.png";
            return ResourceLoader.Exists(custom) ? custom : "res://TheQueen/images/potions/potion_outline.png";
        }
    }
}