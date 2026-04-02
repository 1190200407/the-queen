using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

public sealed class ChainsPrisonPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		// 仅在“对手获得魂灯”时触发。
		if (amount <= 0m || power is not SoulLampPower || power.Owner == base.Owner)
		{
			return;
		}

		Flash();
		await CreatureCmd.Damage(
			new ThrowingPlayerChoiceContext(),
			base.Owner,
			base.Amount,
			ValueProp.Unpowered,
			applier ?? power.Owner,
			cardSource
		);
	}
}
