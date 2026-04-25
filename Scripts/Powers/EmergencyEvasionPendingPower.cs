using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

/// <summary>「紧急避险」：下回合开始时召唤 <see cref="PowerModel.Amount"/> 并 <see cref="FriendlyAmalgam.ClearForcedAction"/>。</summary>
public sealed class EmergencyEvasionPendingPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player.Creature != base.Owner || !base.Owner.IsAlive)
		{
			return;
		}

		CombatState? combatState = base.Owner.CombatState;
		if (combatState == null)
		{
			return;
		}

		Creature? amalgamCreature = FriendlyAmalgamCmd.GetExisting(combatState, player);

		if (amalgamCreature?.Monster is FriendlyAmalgam amalgam)
		{
			await amalgam.ClearForcedAction();
		}
		await FriendlyAmalgamCmd.Summon(choiceContext, player, base.Amount, this);

		await PowerCmd.Remove(this);
	}
}
