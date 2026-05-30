
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace ComicChess.TheQueen;

public abstract class QueenPowerModel : ModPowerTemplate
{
    public override string? CustomBigIconPath =>
        $"res://TheQueen/images/powers/big/{ResolvePowerIconKey()}.png";

    public override string? CustomIconPath =>
        $"res://TheQueen/images/powers/{ResolvePowerIconKey()}.png";

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
