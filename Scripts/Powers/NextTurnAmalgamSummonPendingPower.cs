using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

/// <summary>
/// 通用「下回合开始时召唤聚合体」：与原版下回合召唤 pending 同类，在玩家下回合开始时按 <see cref="PowerModel.Amount"/> 调用 <see cref="FriendlyAmalgamCmd.Summon"/>，然后移除自身。
/// </summary>
public sealed class NextTurnAmalgamSummonPendingPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player.Creature != base.Owner || !base.Owner.IsAlive)
		{
			return;
		}

		ICombatState? combatState = base.Owner.CombatState;
		if (combatState == null)
		{
			return;
		}

		await FriendlyAmalgamCmd.Summon(choiceContext, player, base.Amount, this);
		await PowerCmd.Remove(this);
	}
}
