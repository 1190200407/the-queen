using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using Godot;

namespace ComicChess.TheQueen;

public abstract class QueenPowerModel : CustomPowerModel
{
    public override string? CustomPackedIconPath
    {
        get
        {
            string key = ResolvePowerIconKey();
            string custom = $"res://TheQueen/images/powers/{key}.png";
            return ResourceLoader.Exists(custom) ? custom : "res://TheQueen/images/powers/power.png";
        }
    }

    public override string? CustomBigIconPath
    {
        get
        {
            string key = ResolvePowerIconKey();
            string custom = $"res://TheQueen/images/powers/big/{key}.png";
            return ResourceLoader.Exists(custom) ? custom : "res://TheQueen/images/powers/big/power.png";
        }
    }

    private string ResolvePowerIconKey()
    {
        string key = Id.Entry.ToLowerInvariant().Replace("comicchess-", "");
        if (key.EndsWith("_power"))
        {
            key = key[..^"_power".Length];
        }

        return key;
    }
}