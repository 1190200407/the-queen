using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 未了之祸（<see cref="IsInstanced"/>，挂在敌人身上的 Debuff）：当<strong>该敌人</strong>身上<strong>其它</strong>负面能力被移除（含层数归零）时，
/// 每条本能力实例对所有可攻击敌人各随机施加毒/灾厄/消亡之一（<see cref="QueenCardCmd.ApplyRandomTriadDebuff"/>），层数由打出 <see cref="UnfinishedCalamity"/> 时决定（基础 5，升级 7）。
/// 移除本能力自身不会触发。
/// </summary>
public sealed class UnfinishedCalamityPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	internal static async Task TryApplyAfterEnemyDebuffRemoved(ICombatState combatState, Creature victim, PowerModel removedPower)
	{
		if (removedPower is UnfinishedCalamityPower)
		{
			return;
		}

		if (victim is not { IsAlive: true, IsEnemy: true })
		{
			return;
		}

		List<UnfinishedCalamityPower> marks = victim.GetPowerInstances<UnfinishedCalamityPower>().ToList();
		if (marks.Count == 0)
		{
			return;
		}

		foreach (UnfinishedCalamityPower calamity in marks)
		{
			if (calamity.Amount <= 0)
			{
				continue;
			}

			Creature? dealer = calamity.Applier;
			Player? player = dealer?.Player;
			if (player is null || dealer is not { IsAlive: true })
			{
				continue;
			}

			calamity.Flash();
			await QueenCardCmd.ApplyRandomTriadDebuff(new ThrowingPlayerChoiceContext(), player, victim, dealer, null, calamity.Amount);
		}
	}
}
