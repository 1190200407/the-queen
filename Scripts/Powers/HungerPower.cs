using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

public sealed class HungerPower : QueenPowerModel
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

		if (base.Owner.Player is not { } player)
		{
			return;
		}

		await QueenCardCmd.AddSoulLamp(player, 1);
	}
}
