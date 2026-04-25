using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

/// <summary>友方聚合体扩展事件：行动后、苏醒后、以及造成伤害后的斩杀钩子。</summary>
public static class FriendlyAmalgamHook
{
	/// <summary>
	/// 聚合体对 <paramref name="damagedEnemy"/> 结算伤害后调用；若本次结算中将其斩杀，则触发
	/// <see cref="PredatoryAssimilationPower.TryGrantCaptureAfterAmalgamFatalKillAsync"/> 等基于斩杀的扩展。
	/// </summary>
	public static async Task AfterAmalgamDamagedCreature(
		PlayerChoiceContext choiceContext,
		Creature amalgam,
		Creature damagedEnemy,
		IEnumerable<DamageResult> damageResults)
	{
		if (amalgam.PetOwner is not Player queen)
		{
			return;
		}

		if (damageResults.Any(static r => r.WasTargetKilled))
		{
            PredatoryAssimilationPower? predatoryAssimilation = queen.Creature.GetPower<PredatoryAssimilationPower>();
            if (predatoryAssimilation is not null)
            {
                await predatoryAssimilation.TryGrantCaptureAfterAmalgamFatalKillAsync(choiceContext, queen, damagedEnemy);
            }
		}
	}

    /// <summary>聚合体完成一次行动后触发（用于先锋等效果）。</summary>
    public static async Task AfterAct(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        if (amalgam.PetOwner is not Player queen || queen.Creature is not { IsAlive: true } queenBody)
        {
            return;
        }

        if (queenBody.GetPower<VanguardPower>() is { Amount: > 0 } vanguard)
        {
            await vanguard.OnAmalgamActAsync(choiceContext, queen);
        }

        if (amalgam.GetPower<AmalgamSteamEruptionPower>() is { Amount: > 0 } steamEruption)
        {
            await steamEruption.OnAmalgamActAsync(choiceContext, amalgam);
        }
    }

    /// <summary>聚合体从沉睡状态恢复时触发（用于蛰伏再生等效果）。</summary>
    public static async Task AfterAwake(Creature amalgam)
    {
        if (amalgam.PetOwner is not Player queen || queen.Creature is not { IsAlive: true } queenBody)
        {
            return;
        }

        if (queenBody.GetPower<DormantRebirthPower>() is { Amount: > 0 } dormantRebirth)
        {
            await dormantRebirth.OnAmalgamWakeFromSleepAsync(queen);
        }
    }
}
