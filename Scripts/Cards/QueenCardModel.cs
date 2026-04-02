using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace ComicChess.TheQueen;

public abstract class QueenCardModel : CustomCardModel
{
    //public override string PortraitPath => $"res://TheQueen/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    public override string PortraitPath => $"res://TheQueen/images/card_portraits/card.png";

    /// <summary>
    /// 图鉴、抽牌预览等使用不可变原型，没有 <see cref="CardModel.Affliction"/>。
    /// 为 true 时：<see cref="BoundOverlayPreviewPatch"/> 注入原版魂缚叠层；
    /// <see cref="BoundDescriptionPreviewPatch"/> 在描述末尾追加 <c>COMICCHESS-BOUNDED.description</c>（战斗内已有真实 Bound 时不追加）。
    /// 此类牌<strong>勿在</strong> <c>cards.json</c> 的 description 里再写魂缚行，以免与补丁或 affliction 重复。
    /// </summary>
    internal virtual bool UseBoundAfflictionOverlayForPreview => false;

    public override bool HasBuiltInOverlay => UseBoundAfflictionOverlayForPreview;

    public QueenCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary) : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
}