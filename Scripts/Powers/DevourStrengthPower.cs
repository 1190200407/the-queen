using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content.Patches;
using STS2RitsuLib.Scaffolding.Content;

namespace ComicChess.TheQueen;

/// <summary>吞噬：本回合玩家力量增益（回合结束失去）。</summary>
public sealed class DevourStrengthPower : TemporaryStrengthPower, IModPowerAssetOverrides
{
	public override AbstractModel OriginModel => ModelDb.Card<Devour>();

    public PowerAssetProfile AssetProfile => new(
        IconPath: "res://TheQueen/images/powers/devour_strength.png",
        BigIconPath: "res://TheQueen/images/powers/big/devour_strength.png"
    );

    public string? CustomIconPath => "res://TheQueen/images/powers/devour_strength.png";
    public string? CustomBigIconPath => "res://TheQueen/images/powers/big/devour_strength.png";
}
