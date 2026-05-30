using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ComicChess.TheQueen;

public abstract class QueenCardModel : ModCardTemplate
{
    public override string PortraitPath
    {
        get
        {
            string key = ResolvePortraitKey();
            return Pool switch
            {
                EnemyCardPool => PortraitPathForMonsters(key),
                TokenCardPool => ResolveTokenPortraitPath(key),
                _ => PortraitPathForQueen(key),
            };
        }
    }

    private static string PortraitPathForQueen(string key) =>
        $"res://TheQueen/images/card_portraits/{key}.png";

    private static string PortraitPathForMonsters(string key) =>
        $"res://TheQueen/images/card_portraits/monsters/{key}.png";

    private static string ResolveTokenPortraitPath(string key)
    {
        string queenPath = PortraitPathForQueen(key);
        if (ResourceLoader.Exists(queenPath))
        {
            return queenPath;
        }

        return PortraitPathForMonsters(key);
    }

    private string ResolvePortraitKey() =>
        Id.Entry.ToLowerInvariant().Replace("the_queen_card_", "");

    /// <summary>
    /// 图鉴、抽牌预览等使用不可变原型，没有 <see cref="CardModel.Affliction"/>。
    /// 为 true 时：<see cref="BoundOverlayPreviewPatch"/> 注入原版魂缚叠层；
    /// <see cref="BoundDescriptionPreviewPatch"/> 在描述末尾追加 <c>COMICCHESS-BOUNDED.description</c>（战斗内已有真实 Bound 时不追加）。
    /// 此类牌<strong>勿在</strong> <c>cards.json</c> 的 description 里再写魂缚行，以免与补丁或 affliction 重复。
    /// </summary>
    internal virtual bool HasSelfBound => false;

    private bool ShouldApplySelfBound(CardModel card) =>
        card == this
        && HasSelfBound
        && base.Owner?.Creature?.CombatState != null;

    private Task TryApplySelfBound(CardModel card)
    {
        if (!ShouldApplySelfBound(card))
        {
            return Task.CompletedTask;
        }

        CardCmd.ClearAffliction(card);
        return CardCmd.Afflict<Bound>(this, 1m);
    }

    public override Task BeforeCombatStart()
    {
        return TryApplySelfBound(this);
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        return TryApplySelfBound(card);
    }

    /// <summary>
    /// 无战斗上下文（图鉴、预构）仍为 true，由 <see cref="BoundOverlayPreviewPatch"/> 提供魂缚叠层。
    /// 战斗内仅在仍存在 <see cref="Bound"/> 时为 true；清除侵蚀后须为 false，否则 <see cref="MegaCrit.Sts2.Core.Nodes.Cards.NCard"/> 的
    /// <c>ReloadOverlay</c> 会在 <c>Affliction == null</c> 时仍走内置叠层分支，看起来像清不掉。
    /// </summary>
    public override bool HasBuiltInOverlay =>
        (HasSelfBound || Enchantment is SoulLight) && (CombatState == null || Affliction is Bound);

    public QueenCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary) : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
}
