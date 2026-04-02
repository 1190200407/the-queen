using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

public sealed class AshenWallPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.Static(StaticHoverTip.Block),
		HoverTipFactory.FromKeyword(QueenKeyword.fade)
	];

	public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		if (card.Owner?.Creature != base.Owner)
		{
			return;
		}
		if (!card.Keywords.Contains(QueenKeyword.fade))
		{
			return;
		}

		await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
	}
}
