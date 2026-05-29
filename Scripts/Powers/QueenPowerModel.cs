
using MegaCrit.Sts2.Core.Entities.Cards;
using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace ComicChess.TheQueen;

public abstract class QueenPowerModel : ModPowerTemplate
{
    public override string? CustomBigIconPath
    {
        get
        {
            string key = ResolvePowerIconKey();
            string custom = $"res://TheQueen/images/powers/big/{key}.png";
            return ResourceLoader.Exists(custom) ? custom : "res://TheQueen/images/powers/big/power.png";
        }
    }

    public override string? CustomIconPath
    {
        get
        {
            string key = ResolvePowerIconKey();
            string custom = $"res://TheQueen/images/powers/{key}.png";
            return ResourceLoader.Exists(custom) ? custom : "res://TheQueen/images/powers/power.png";
        }
    }

    private string ResolvePowerIconKey()
    {
        string key = Id.Entry.ToLowerInvariant().Replace("the_queen_power_", "");
        if (key.EndsWith("_power"))
        {
            key = key[..^"_power".Length];
        }

        return key;
    }
}