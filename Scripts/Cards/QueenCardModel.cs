using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;

namespace ComicChess.TheQueen;

public abstract class QueenCardModel : CustomCardModel
{
	private static readonly PileType[] PilesForSoulLampBroadcast =
	[
		PileType.Hand,
		PileType.Draw,
		PileType.Discard,
		PileType.Exhaust,
		PileType.Play
	];

    public override string PortraitPath
    {
        get
        {
            string portraitPath = $"res://TheQueen/images/card_portraits/{Id.Entry.ToLowerInvariant().Replace("comicchess-", "")}.png";
            if (ResourceLoader.Exists(portraitPath))
            {
                return portraitPath;
            }
            else
            {
                portraitPath = $"res://TheQueen/images/card_portraits/monsters/{Id.Entry.ToLowerInvariant().Replace("comicchess-", "")}.png";
                if (ResourceLoader.Exists(portraitPath))
                {
                    return portraitPath;
                }
            }

            return "res://TheQueen/images/card_portraits/card.png";
        }
    }

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
        && base.Owner?.Creature?.CombatState != null
        && Affliction is null;

    private Task TryApplySelfBound(CardModel card)
    {
        if (!ShouldApplySelfBound(card))
        {
            return Task.CompletedTask;
        }

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
    /// 拥有此牌的玩家的 <see cref="SoulLampPower"/> 层数变化时由引擎路径广播（见 <see cref="BroadcastSoulLampAmountChange"/>）。
    /// <paramref name="delta"/> &gt; 0 为获得魂灯，&lt; 0 为失去（如打出魂缚牌消耗）。
    /// </summary>
    public virtual Task OnSoulLampAmountChange(Player player, decimal delta, Creature? applier, CardModel? cardSource) =>
        Task.CompletedTask;

    /// <summary>
    /// <see cref="SoulLampPower"/> 在层数变化时调用：对该玩家各牌堆中的 <see cref="QueenCardModel"/> 逐个派发。
    /// </summary>
    internal static async Task BroadcastSoulLampAmountChange(Player player, decimal delta, Creature? applier, CardModel? cardSource)
    {
        foreach (PileType pileType in PilesForSoulLampBroadcast)
        {
            CardPile pile = pileType.GetPile(player);
            foreach (CardModel card in pile.Cards.ToList())
            {
                if (card.Owner != player)
                {
                    continue;
                }

                if (card is QueenCardModel queen)
                {
                    await queen.OnSoulLampAmountChange(player, delta, applier, cardSource);
                }
            }
        }
    }

    public override bool HasBuiltInOverlay => HasSelfBound;

    public QueenCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary) : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
}