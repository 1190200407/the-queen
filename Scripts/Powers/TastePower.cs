using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>尝味：本回合打出吞噬时抽牌；回合结束时移除。</summary>
public sealed class TastePower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		CardModel played = cardPlay.Card;
		if (played is not Devour || played.Owner?.Creature != base.Owner)
		{
			return;
		}

		Player? player = base.Owner.Player;
		if (player == null)
		{
			return;
		}

		int n = (int)Amount;
		if (n > 0)
		{
			await CardPileCmd.Draw(context, n, player);
		}
	}

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
		_ = choiceContext;
		if (side == base.Owner.Side)
		{
			await PowerCmd.Remove(this);
		}
	}
}
