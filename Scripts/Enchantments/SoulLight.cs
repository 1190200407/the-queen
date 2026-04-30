using System;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 附魔「灯火」：打出该牌时获得魂灯（层数等于附魔等级）。
/// </summary>
public sealed class SoulLight : CustomEnchantmentModel
{
    public override bool ShowAmount => true;
    public override bool HasExtraCardText => true;

	protected override string? CustomIconPath => "res://TheQueen/images/powers/soul_lamp.png";
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [QueenHoverTips.SoulLamp];

    public override bool CanEnchant(CardModel card)
    {
        return card.CanPlay();
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		_ = choiceContext;
		_ = cardPlay;
		if (base.Card?.Owner == null)
		{
			return;
		}

		
        if (Status == EnchantmentStatus.Normal)
		{
			await QueenCardCmd.AddSoulLamp(base.Card.Owner, base.Amount);
			Status = EnchantmentStatus.Disabled;
		}
	}
}
