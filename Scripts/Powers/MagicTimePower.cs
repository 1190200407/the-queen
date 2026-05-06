using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;

namespace ComicChess.TheQueen;

/// <summary>
/// 魂灯为 0 时消耗 1 点能量并获得 1 点魂灯（含 -1 哨兵视为 0）。
/// 触发：回合开始、获得能量后、魂灯层数下降后（见 <see cref="MagicTimeGainEnergyPatch"/>、<see cref="SoulLampPower"/>）。
/// 层数：每层各尝试一次。
/// </summary>
public sealed class MagicTimePower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
		CardCmd.ClearAffliction(card);
        await CardCmd.Afflict<Bound>(card, 1m);
    }

    internal static async Task TryAutoRefillSoulLamp(Player player)
	{
		if (player?.Creature == null || player.PlayerCombatState == null)
		{
			return;
		}

		Creature creature = player.Creature;
		MagicTimePower? magicTime = creature.GetPower<MagicTimePower>();
		if (magicTime == null || magicTime.Amount <= 0)
		{
			return;
		}

		for (int i = 0; i < magicTime.Amount; i++)
		{
			SoulLampPower? lamp = creature.GetPower<SoulLampPower>();
			if (lamp != null && lamp.Amount > 0)
			{
				return;
			}

			if (player.PlayerCombatState.Energy < 1)
			{
				return;
			}

			await PlayerCmd.LoseEnergy(1m, player);
			await QueenCardCmd.AddSoulLamp(player, 1);
		}
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		_ = choiceContext;
		if (player != base.Owner.Player)
		{
			return;
		}
		await TryAutoRefillSoulLamp(player);
	}
}
