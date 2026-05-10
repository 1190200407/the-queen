using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>苦痛之路（可叠加）：你每打出一张牌，对一名随机敌人随机施加等同于层数的毒/灾厄/消亡（<see cref="QueenCardCmd.ApplyRandomTriadDebuff"/>）。</summary>
public sealed class PathOfPainPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = choiceContext;
		CardModel played = cardPlay.Card;
		if (played.Owner?.Creature != base.Owner)
		{
			return;
		}

		Player? player = base.Owner.Player;
		if (player == null || base.CombatState is not CombatState combatState)
		{
			return;
		}

		List<Creature> enemies = combatState.HittableEnemies.ToList();
		if (enemies.Count == 0)
		{
			return;
		}

		Creature? target = FriendlyAmalgamCmd.NextRandomHittableEnemy(player, enemies);
		if (target == null)
		{
			return;
		}

		if (Amount <= 0)
		{
			return;
		}

		await QueenCardCmd.ApplyRandomTriadDebuff(player, target, base.Owner, played, Amount);
	}
}
