using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace ComicChess.TheQueen;

[HarmonyPatch]
internal static class QueenSummonOnCombatStartPatch
{
    /// <summary>
    /// 战斗开始后，如果有女王玩家，则生成 0 当前生命、最大生命已按遭遇缩放的聚合体壳（不治疗、不占召唤历史）。
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
    private static async Task BeforeCombatStart_Postfix(Task __result, IRunState runState, CombatState? combatState)
    {
        _ = runState;
        await __result;

        if (combatState == null)
        {
            return;
        }

        var choiceContext = new BlockingPlayerChoiceContext();
        foreach (Player player in combatState.Players)
        {
            if (player.Character is QueenCharacter)
            {
                await FriendlyAmalgamCmd.EnsureAmalgamCombatStartShellAsync(choiceContext, player);
            }
        }
    }
}

