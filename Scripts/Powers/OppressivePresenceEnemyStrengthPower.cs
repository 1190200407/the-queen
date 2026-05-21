using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace ComicChess.TheQueen;

/// <summary>压迫感：使目标本回合失去力量（回合结束恢复）。</summary>
public sealed class OppressivePresenceEnemyStrengthPower : TemporaryStrengthPower, IModPowerAssetOverrides
{
	public override AbstractModel OriginModel => ModelDb.Card<OppressivePresence>();

    public PowerAssetProfile AssetProfile => new(
        IconPath: "res://TheQueen/images/powers/suppress_strength_down.png",
        BigIconPath: "res://TheQueen/images/powers/big/suppress_strength_down.png"
    );

	public string? CustomIconPath => "res://TheQueen/images/powers/suppress_strength_down.png";
	public string? CustomBigIconPath => "res://TheQueen/images/powers/big/suppress_strength_down.png";

    protected override bool IsPositive => false;
}
