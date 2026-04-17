using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace ComicChess.TheQueen;

// [HarmonyPatch]
// internal static class QueenSummonOnCombatStartPatch
// {
//     /// <summary>
//     /// 战斗开始后，如果有女王玩家，则为其召唤 1 点生命值的聚合体。
//     /// 独立于魂缚逻辑，不修改任何原有 Hook 行为。
//     /// </summary>
//     [HarmonyPostfix]
//     [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
//     private static async Task BeforeCombatStart_Postfix(Task __result, IRunState runState, CombatState? combatState)
//     {
//         _ = runState;
//         await __result;

//         if (combatState == null)
//         {
//             return;
//         }

//         var choiceContext = new BlockingPlayerChoiceContext();
//         foreach (Player player in combatState.Players)
//         {
//             if (player.Character is QueenCharacter)
//             {
//                 await FriendlyAmalgamCmd.Summon(choiceContext, player, 1m, null);
//             }
//         }
//     }
// }

