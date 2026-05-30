using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

public interface ISoulLampEventListener
{
	/// <param name="delta">&gt; 0 为获得魂灯，&lt; 0 为失去（如打出魂缚牌消耗）。</param>
	Task OnSoulLampAmountChanged(
		PlayerChoiceContext choiceContext,
		Player player,
		decimal delta,
		Creature? applier,
		CardModel? cardSource)
	{
		return Task.CompletedTask;
	}
}
