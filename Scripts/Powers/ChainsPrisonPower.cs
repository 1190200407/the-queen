using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

public sealed class ChainsPrisonPower : QueenPowerModel, ISoulLampEventListener
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public async Task OnSoulLampAmountChanged(
		PlayerChoiceContext choiceContext,
		Player player,
		decimal delta,
		Creature? applier,
		CardModel? cardSource)
	{
		// 仅在“对手获得魂灯”时触发。
		if (delta <= 0m || player.Creature == base.Owner)
		{
			return;
		}

		Flash();
		await CreatureCmd.Damage(
			new ThrowingPlayerChoiceContext(),
			base.Owner,
			base.Amount,
			ValueProp.Unpowered,
			applier ?? player.Creature,
			cardSource,
			null
		);
	}
}
