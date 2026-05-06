using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 隐藏机制：场上已有聚合体时，原版 <see cref="OstyCmd.Summon"/> 改走 <see cref="FriendlyAmalgamCmd.Summon"/>。
/// 「有奥斯提则聚合体召唤改奥斯提」写在 <see cref="FriendlyAmalgamCmd.Summon"/> 内，无需再 patch 自身。
/// </summary>
internal static class OstyAmalgamSummonCrossRedirectPatch
{
	[HarmonyPatch(typeof(OstyCmd), nameof(OstyCmd.Summon))]
	private static class OstyCmdSummonToAmalgamWhenAmalgamExists
	{
		[HarmonyPriority(Priority.First)]
		[HarmonyPrefix]
		private static bool Prefix(
			PlayerChoiceContext choiceContext,
			Player summoner,
			decimal amount,
			AbstractModel? source,
			ref Task<SummonResult> __result)
		{
			if (summoner.Creature.CombatState is not { } combatState)
			{
				return true;
			}

			if (FriendlyAmalgamCmd.GetExisting(combatState, summoner) == null)
			{
				return true;
			}

			__result = SummonAmalgamInsteadAsync(choiceContext, summoner, amount, source);
			return false;
		}

		private static async Task<SummonResult> SummonAmalgamInsteadAsync(
			PlayerChoiceContext choiceContext,
			Player summoner,
			decimal amount,
			AbstractModel? source)
		{
			await FriendlyAmalgamCmd.Summon(choiceContext, summoner, amount, source);
			CombatState? cs = summoner.Creature.CombatState;
			Creature? amalgam = cs == null ? null : FriendlyAmalgamCmd.GetExisting(cs, summoner);
			return new SummonResult(amalgam, amount);
		}
	}
}
