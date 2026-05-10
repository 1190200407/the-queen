using BaseLib.Abstracts;
using Godot;

public abstract class QueenPotionModel : CustomPotionModel
{
    public override string? CustomPackedImagePath
    {
        get
        {
            string key = Id.Entry.ToLowerInvariant().Replace("comicchess-", "");
            string custom = $"res://TheQueen/images/potions/{key}.png";
            return ResourceLoader.Exists(custom) ? custom : "res://TheQueen/images/potions/potion.png";
        }
    }
    public override string? CustomPackedOutlinePath
    {
        get
        {
            string key = Id.Entry.ToLowerInvariant().Replace("comicchess-", "");
            string custom = $"res://TheQueen/images/potions/{key}_outline.png";
            return ResourceLoader.Exists(custom) ? custom : "res://TheQueen/images/potions/potion_outline.png";
        }
    }
}