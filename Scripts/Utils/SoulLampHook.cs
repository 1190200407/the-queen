using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>魂灯层数变化时，经 <see cref="ICombatState.IterateHookListeners"/> 派发给 <see cref="ISoulLampEventListener"/>。</summary>
public static class SoulLampHook
{
	public static async Task AfterAmountChanged(
		ICombatState combatState,
		PlayerChoiceContext choiceContext,
		Player player,
		decimal delta,
		Creature? applier,
		CardModel? cardSource)
	{
		if (delta == 0m)
		{
			return;
		}

		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			if (item is ISoulLampEventListener listener)
			{
				await listener.OnSoulLampAmountChanged(choiceContext, player, delta, applier, cardSource);
			}
		}
	}
}
