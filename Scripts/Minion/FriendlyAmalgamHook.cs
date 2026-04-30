using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>友方聚合体扩展事件：行动后、苏醒后、以及造成伤害后的斩杀钩子。</summary>
public static class FriendlyAmalgamHook
{
	/// <summary>
	/// 聚合体对 <paramref name="damagedEnemy"/> 结算伤害后调用；若本次结算中将其斩杀，则触发
	/// <see cref="PredatoryAssimilationPower.TryGrantCaptureAfterAmalgamFatalKillAsync"/> 等基于斩杀的扩展。
	/// </summary>
	public static async Task AfterAmalgamDamagedCreature(
        CombatState combatState,
		PlayerChoiceContext choiceContext,
		Creature amalgam,
		Creature damagedEnemy,
		IEnumerable<DamageResult> damageResults)
	{
		foreach (AbstractModel item in combatState.IterateHookListeners())
        {
            if (item is IAmalgamEventListener listener)
            {
                await listener.OnAmalgamDamagedCreatureAsync(combatState, choiceContext, amalgam, damagedEnemy, damageResults);
            }
        }
	}

    /// <summary>聚合体完成一次行动后触发（用于先锋等效果）。</summary>
    public static async Task AfterAct(CombatState combatState, PlayerChoiceContext choiceContext, Creature amalgam)
    {
        if (amalgam.PetOwner is not Player queen)
        {
            return;
        }
        
        foreach (AbstractModel item in combatState.IterateHookListeners())
        {
            if (item is IAmalgamEventListener listener)
            {
                await listener.OnAmalgamActAsync(combatState, choiceContext, amalgam);
            }
        }
    }

    /// <summary>聚合体从沉睡状态恢复时触发（用于蛰伏再生等效果）。</summary>
    public static async Task AfterAwake(CombatState queenCombatState, Creature amalgam)
    {
        foreach (AbstractModel item in queenCombatState.IterateHookListeners())
        {
            if (item is IAmalgamEventListener listener)
            {
                await listener.OnAmalgamWakeFromSleepAsync(queenCombatState, amalgam);
            }
        }
    }

    /// <summary>聚合体逃跑后触发。</summary>
    public static async Task OnEscape(CombatState combatState, Creature amalgam)
    {
        foreach (AbstractModel item in combatState.IterateHookListeners())
        {
            if (item is IAmalgamEventListener listener)
            {
                await listener.OnAmalgamEscapeAsync(combatState, amalgam);
            }
        }
    }
}
