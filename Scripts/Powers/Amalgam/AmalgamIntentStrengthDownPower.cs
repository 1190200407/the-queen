using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace ComicChess.TheQueen;

/// <summary>通用：聚合体“学习意图”造成的本回合力量降低（回合结束恢复）。</summary>
public sealed class AmalgamIntentStrengthDownPower : TemporaryStrengthPower, IModPowerAssetOverrides
{
    public PowerAssetProfile AssetProfile => new(
        IconPath: "res://TheQueen/images/powers/amalgam_intent_strength_down.png",
        BigIconPath: "res://TheQueen/images/powers/big/amalgam_intent_strength_down.png"
    );

    public string? CustomIconPath => "res://TheQueen/images/powers/amalgam_intent_strength_down.png";
    public string? CustomBigIconPath => "res://TheQueen/images/powers/big/amalgam_intent_strength_down.png";

    public override AbstractModel OriginModel => ModelDb.Monster<FriendlyAmalgam>();

    public override LocString Title => new("powers", "AMALGAM_INTENT_STRENGTH_DOWN_POWER.title");

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    protected override bool IsPositive => false;
}


