using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

public sealed class SoulMoldPower : QueenPowerModel
{
	private sealed class Data
	{
		public bool triggeredThisTurn;
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override object InitInternalData() => new Data();

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		CardModel played = cardPlay.Card;
		if (played.Owner?.Creature != base.Owner)
		{
			return;
		}
		if (played.Type != CardType.Skill)
		{
			return;
		}

		Data data = GetInternalData<Data>();
		if (data.triggeredThisTurn)
		{
			return;
		}
		data.triggeredThisTurn = true;

		CombatState? combatState = base.Owner.CombatState;
		if (combatState == null)
		{
			return;
		}

		CardModel copy = combatState.CloneCard(played);
		await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, addedByPlayer: true);
		CardCmd.ApplyKeyword(copy, QueenKeyword.fade);

		if (copy.Affliction is not null && copy.Affliction is not Bound)
		{
			CardCmd.ClearAffliction(copy);
		}
		if (copy.Affliction is not Bound)
		{
			await CardCmd.Afflict<Bound>(copy, 1m);
		}
	}

	public override Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side)
		{
			GetInternalData<Data>().triggeredThisTurn = false;
		}
		return Task.CompletedTask;
	}
}
