using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 附魔「灯火」：打出该牌时获得魂灯（层数等于附魔等级）。
/// </summary>
public sealed class SoulLight : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		_ = choiceContext;
		_ = cardPlay;
		if (base.Card?.Owner == null)
		{
			return;
		}

		await QueenCardCmd.AddSoulLamp(base.Card.Owner, base.Amount);
	}
}
