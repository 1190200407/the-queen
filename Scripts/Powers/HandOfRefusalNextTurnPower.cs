using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace ComicChess.TheQueen;

public sealed class HandOfRefusalNextTurnPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public bool isUpgraded = false;

	protected override bool IsVisibleInternal => false;

	public override async Task AfterEnergyReset(Player player)
	{
		if (player != base.Owner.Player)
		{
			return;
		}

		if (base.Amount > 0 && base.CombatState != null)
		{
			for (int i = 0; i < base.Amount; i++)
			{
				await QueenCardCmd.CreateInHand<HandOfRefusal>(player, base.CombatState, isUpgraded: isUpgraded);
			}
		}

		await PowerCmd.Remove(this);
	}
}
