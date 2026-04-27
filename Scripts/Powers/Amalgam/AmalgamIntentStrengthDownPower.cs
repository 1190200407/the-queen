using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>通用：聚合体“学习意图”造成的本回合力量降低（回合结束恢复）。</summary>
public sealed class AmalgamIntentStrengthDownPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Monster<FriendlyAmalgam>();

    public override LocString Title => new("powers", "AMALGAM_INTENT_STRENGTH_DOWN_POWER.title");

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    protected override bool IsPositive => false;
}


