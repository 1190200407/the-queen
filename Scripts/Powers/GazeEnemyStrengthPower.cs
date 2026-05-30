using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content.Patches;
using STS2RitsuLib.Scaffolding.Content;

namespace ComicChess.TheQueen;

/// <summary>注视：目标本回合失去力量（回合结束恢复）。</summary>
public sealed class GazeEnemyStrengthPower : TemporaryStrengthPower, IModPowerAssetOverrides
{
    public PowerAssetProfile AssetProfile => new(
        IconPath: "res://TheQueen/images/powers/gaze_strength_down.png",
        BigIconPath: "res://TheQueen/images/powers/big/gaze_strength_down.png"
    );

    public string? CustomIconPath => "res://TheQueen/images/powers/gaze_strength_down.png";
    public string? CustomBigIconPath => "res://TheQueen/images/powers/big/gaze_strength_down.png";

	public override AbstractModel OriginModel => ModelDb.Card<Gaze>();

	protected override bool IsPositive => false;
}
