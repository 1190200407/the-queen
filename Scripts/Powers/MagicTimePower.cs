using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

/// <summary>
/// 回合开始时：若魂灯为 0（含显示为 0 的 -1 哨兵），且有至少 1 点能量，则失去 1 能量并获得 1 魂灯。
/// 层数：每层各尝试一次（与 <see cref="WillfulPower"/> 的回合开始魂灯类似）。
/// </summary>
public sealed class MagicTimePower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		_ = choiceContext;
		if (player != base.Owner.Player)
		{
			return;
		}

		for (int i = 0; i < base.Amount; i++)
		{
			SoulLampPower? lamp = base.Owner.GetPower<SoulLampPower>();
			if (lamp != null && lamp.Amount > 0)
			{
				continue;
			}

			if (player.PlayerCombatState == null || player.PlayerCombatState.Energy < 1)
			{
				return;
			}

			await PlayerCmd.LoseEnergy(1m, player);
			await QueenCardCmd.AddSoulLamp(player, 1);
		}
	}
}
