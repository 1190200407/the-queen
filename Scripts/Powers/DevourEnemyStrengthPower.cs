using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace ComicChess.TheQueen;

/// <summary>吞噬：目标本回合失去力量（回合结束恢复）。</summary>
public sealed class DevourEnemyStrengthPower : TemporaryStrengthPower, IModPowerAssetOverrides
{
    public PowerAssetProfile AssetProfile => new(
        IconPath: "res://TheQueen/images/powers/devour_strength.png",
        BigIconPath: "res://TheQueen/images/powers/big/devour_strength.png");

    public string? CustomIconPath => "res://TheQueen/images/powers/devour_strength.png";
    public string? CustomBigIconPath => "res://TheQueen/images/powers/big/devour_strength.png";

    public override AbstractModel OriginModel => ModelDb.Card<Devour>();

    protected override bool IsPositive => false;
}
