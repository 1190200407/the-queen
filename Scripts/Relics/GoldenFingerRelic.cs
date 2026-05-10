using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace ComicChess.TheQueen;

/// <summary>金手指：每回合每累计 {PlaysRequired} 次未消耗能量打出卡牌，获得 {Energy} 点能量并抽 {Cards} 张牌。</summary>
[Pool(typeof(QueenRelicPool))]
public sealed class GoldenFingerRelic : QueenRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool ShowCounter => true;

	public override int DisplayAmount => _zeroEnergyPlaysThisTurn;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new IntVar("PlaysRequired", 3m),
		new IntVar("Energy", 1m),
		new CardsVar(1),
	];

	private int _zeroEnergyPlaysThisTurn;

	public override Task BeforeCombatStart()
	{
		_zeroEnergyPlaysThisTurn = 0;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		_ = choiceContext;
		_ = combatState;
		if (side == base.Owner.Creature.Side)
		{
			_zeroEnergyPlaysThisTurn = 0;
			InvokeDisplayAmountChanged();
		}

		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}

		CardModel played = cardPlay.Card;
		if (played.Owner != base.Owner)
		{
			return;
		}

		Player player = base.Owner;

		if (cardPlay.Resources.EnergySpent != 0)
		{
			return;
		}

		int required = base.DynamicVars["PlaysRequired"].IntValue;
		_zeroEnergyPlaysThisTurn++;
		InvokeDisplayAmountChanged();
		if (_zeroEnergyPlaysThisTurn < required)
		{
			return;
		}

		_zeroEnergyPlaysThisTurn -= required;
		InvokeDisplayAmountChanged();
		Flash();
		int energy = base.DynamicVars["Energy"].IntValue;
		if (energy > 0)
		{
			await PlayerCmd.GainEnergy(energy, player);
		}

		int draw = base.DynamicVars["Cards"].IntValue;
		if (draw > 0)
		{
			await CardPileCmd.Draw(context, draw, player);
		}
	}
}
