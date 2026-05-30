using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;

namespace ComicChess.TheQueen;

/// <summary>
/// 附魔「灯火」：打出该牌时获得魂灯（层数等于附魔等级）。
/// 携带此附魔的牌视为待魂缚（局外预览，战斗内施加真实 <see cref="Bound"/>）。
/// </summary>
public sealed class SoulLight : QueenEnchantmentModel
{
	public override bool ShowAmount => true;
	public override bool HasExtraCardText => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SoulLampPower>()];

	public override async Task BeforeCombatStart()
	{
		await TryApplyBoundOnHostCardAsync();
	}

	public override async Task AfterCardEnteredCombat(CardModel card)
	{
		if (card != base.Card || card.Affliction != null)
		{
			return;
		}

		await CardCmd.Afflict<Bound>(card, 1m);
	}

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		_ = cardPlay;
		if (base.Card?.Owner == null)
		{
			return;
		}

		if (Status == EnchantmentStatus.Normal)
		{
			await QueenCardCmd.AddSoulLamp(choiceContext, base.Card.Owner, base.Amount);
			Status = EnchantmentStatus.Disabled;
		}
	}

	private async Task TryApplyBoundOnHostCardAsync()
	{
		CardModel? card = base.Card;
		if (card == null || card.Affliction != null || card.Owner?.Creature?.CombatState == null)
		{
			return;
		}

		await CardCmd.Afflict<Bound>(card, 1m);
	}
}
