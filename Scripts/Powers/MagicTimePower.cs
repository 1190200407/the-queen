using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;

namespace ComicChess.TheQueen;

/// <summary>
/// 魂灯为 0 时消耗 1 点能量并获得 1 点魂灯。
/// 触发：回合开始、获得能量后（<see cref="MagicTimeGainEnergyPatch"/>）、魂灯层数下降（<see cref="ISoulLampEventListener"/>）。
/// </summary>
public sealed class MagicTimePower : QueenPowerModel, ISoulLampEventListener
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override async Task AfterCardEnteredCombat(CardModel card)
	{
		if (card.Owner != base.Owner?.Player)
		{
			return;
		}

		CardCmd.ClearAffliction(card);
		await CardCmd.Afflict<Bound>(card, 1m);
	}

	public async Task OnSoulLampAmountChanged(
		PlayerChoiceContext choiceContext,
		Player player,
		decimal delta,
		Creature? applier,
		CardModel? cardSource)
	{
		_ = applier;
		_ = cardSource;
		if (delta >= 0m || player != base.Owner?.Player)
		{
			return;
		}

		await TryRefillIfNeeded(choiceContext, player);
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != base.Owner?.Player)
		{
			return;
		}

		await TryRefillIfNeeded(choiceContext, player);
	}

	internal async Task TryRefillIfNeeded(PlayerChoiceContext choiceContext, Player player)
	{
		if (player?.Creature == null || player.PlayerCombatState == null || player != base.Owner?.Player)
		{
			return;
		}

		if (base.Amount <= 0)
		{
			return;
		}

		SoulLampPower? lamp = player.Creature.GetPower<SoulLampPower>();
		if (lamp != null && lamp.Amount > 0)
		{
			return;
		}

		if (player.PlayerCombatState.Energy < 1)
		{
			return;
		}

		await PlayerCmd.LoseEnergy(1m, player);
		await QueenCardCmd.AddSoulLamp(choiceContext, player, 1);
	}
}
