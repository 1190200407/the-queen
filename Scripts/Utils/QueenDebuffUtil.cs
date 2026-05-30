using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 负面效果判定：与原版 Rend / Misery 一致，使用 <see cref="PowerModel.TypeForCurrentAmount"/>（含负力量等 AllowNegative 能力）。
/// </summary>
public static class QueenDebuffUtil
{
	/// <summary>按当前层数是否为 Debuff（例如力量 -2 视为负面，力量 +3 不算）。</summary>
	public static bool IsDebuff(PowerModel power) =>
		power.TypeForCurrentAmount == PowerType.Debuff;

	/// <summary>与原版 Rend 计数一致：Debuff 且非 <see cref="ITemporaryPower"/>。</summary>
	public static bool IsNonTemporaryDebuff(PowerModel power) =>
		IsDebuff(power) && power is not ITemporaryPower;

	public static int CountDebuffPowers(IEnumerable<PowerModel> powers, bool excludeTemporary) =>
		powers.Count(excludeTemporary ? IsNonTemporaryDebuff : IsDebuff);

	public static int CountDebuffPowers(Creature creature, bool excludeTemporary = false) =>
		CountDebuffPowers(creature.Powers, excludeTemporary);

	public static int CountBattlefieldDebuffs(CombatState combatState, bool excludeTemporary = true)
	{
		int n = 0;
		foreach (Creature creature in combatState.Creatures)
		{
			if (!creature.IsAlive)
			{
				continue;
			}

			n += CountDebuffPowers(creature, excludeTemporary);
		}

		return n;
	}
}
