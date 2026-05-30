using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace ComicChess.TheQueen;

/// <summary>知识恶魔诅咒：从 <see cref="Rejuvenate"/> / <see cref="MindClarity"/> / <see cref="Disintegration"/> 三选一并执行。</summary>
internal static class KnowledgeDemonCurseSelection
{
	internal static List<CardModel> CreateCandidateCards(ICombatState combatState, Player player)
	{
		List<CardModel> cards =
		[
			combatState.CreateCard<Rejuvenate>(player),
			combatState.CreateCard<MindClarity>(player),
			combatState.CreateCard<Disintegration>(player),
		];

		return cards.Where(static c => c is KnowledgeDemon.IChoosable).ToList();
	}

	internal static async Task ChooseAndExecuteAsync(
		ICombatState combatState,
		Player player,
		PlayerChoiceContext? choiceContext = null)
	{
		if (!player.Creature.IsAlive)
		{
			return;
		}

		List<CardModel> cards = CreateCandidateCards(combatState, player);
		if (cards.Count == 0)
		{
			return;
		}

		PlayerChoiceContext context = choiceContext ?? new BlockingPlayerChoiceContext();
		CardModel? chosen = await CardSelectCmd.FromChooseACardScreen(context, cards, player);
		if (chosen is KnowledgeDemon.IChoosable choosable)
		{
			await choosable.OnChosen();
		}
	}
}
