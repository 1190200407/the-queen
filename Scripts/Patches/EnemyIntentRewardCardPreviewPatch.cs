using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Rooms;

namespace ComicChess.TheQueen;

internal static class CaptureSingleTargetPreviewEntry
{
    internal static void TryBeginForCardPlay(NCardPlay play, TargetType targetType)
    {
        if (targetType != TargetType.AnyEnemy)
        {
            return;
        }

        if (play is NMouseCardPlay mousePlay)
        {
            CancellationTokenSource? cts = Traverse.Create(mousePlay)
                .Field<CancellationTokenSource>("_cancellationTokenSource")
                .Value;
            if (cts is not null && cts.IsCancellationRequested)
            {
                return;
            }
        }

        CardModel? card = Traverse.Create(play).Property("Card").GetValue<CardModel>();
        if (card is null || !QueenHoverTips.CardHasCaptureHoverTip(card))
        {
            return;
        }

        Player? owner = card.Owner;
        CombatState? combat = card.CombatState;
        if (owner is null || combat is null)
        {
            return;
        }

        IReadOnlyList<Creature> enemies = combat.GetOpponentsOf(owner.Creature).Where(static c => c.IsHittable).ToList();
        EnemyIntentRewardCardPreview.ShowAllRewardCards(owner, enemies, CaptureRewardPreviewRules.TryCreatePreviewCard);
    }
}

[HarmonyPatch(typeof(NMouseCardPlay), "SingleCreatureTargeting")]
internal static class EnemyIntentRewardCardPreview_NMouseCardPlay_SingleCreatureTargeting_Patch
{
    [HarmonyPrefix]
    private static void Prefix(NMouseCardPlay __instance, TargetMode targetMode, TargetType targetType)
    {
        CaptureSingleTargetPreviewEntry.TryBeginForCardPlay(__instance, targetType);
    }
}

[HarmonyPatch(typeof(NControllerCardPlay), "SingleCreatureTargeting")]
internal static class EnemyIntentRewardCardPreview_NControllerCardPlay_SingleCreatureTargeting_Patch
{
    [HarmonyPrefix]
    private static void Prefix(NControllerCardPlay __instance, TargetType targetType)
    {
        CaptureSingleTargetPreviewEntry.TryBeginForCardPlay(__instance, targetType);
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard.SetPreviewTarget))]
internal static class EnemyIntentRewardCardPreview_NCard_SetPreviewTarget_Patch
{
    [HarmonyPostfix]
    private static void Postfix(NCard __instance, Creature? creature)
    {
        CardModel? played = __instance.Model;
        if (played is null || !QueenHoverTips.CardHasCaptureHoverTip(played))
        {
            return;
        }

        EnemyIntentRewardCardPreview.SyncEnlargeWithPreviewTarget(creature);
    }
}

[HarmonyPatch(typeof(NCardPlay), "Cleanup")]
internal static class EnemyIntentRewardCardPreview_NCardPlay_Cleanup_Patch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        EnemyIntentRewardCardPreview.HideAllRewardCards();
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatEnd))]
internal static class EnemyIntentRewardCardPreview_Hook_AfterCombatEnd_Patch
{
    [HarmonyPostfix]
    private static async Task Postfix(Task __result, IRunState runState, CombatState? combatState, CombatRoom room)
    {
        _ = runState;
        _ = combatState;
        _ = room;
        await __result;
        EnemyIntentRewardCardPreview.ResetAfterCombat();
    }
}
