using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace ComicChess.TheQueen;

/// <summary>
/// 放生：拦截主牌组入组（AddInternal）并阻止加入；在奖励结算后执行移除敌怪卡选择。
/// </summary>
[HarmonyPatch]
internal static class ReleasePickupPatch
{
    private static readonly Dictionary<Player, int> PendingPickByPlayer = new();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CardPile), nameof(CardPile.AddInternal))]
    private static bool DeckAddInternal_Prefix(CardPile __instance, CardModel card, int index, bool silent)
    {
        _ = index;
        _ = silent;
        if (__instance.Type != PileType.Deck)
        {
            return true;
        }

        if (card is not Release || card.Owner == null)
        {
            return true;
        }

        int pick = card.IsUpgraded ? 2 : 1;
        if (PendingPickByPlayer.TryGetValue(card.Owner, out int existing))
        {
            PendingPickByPlayer[card.Owner] = existing + pick;
        }
        else
        {
            PendingPickByPlayer[card.Owner] = pick;
        }

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterRewardTaken))]
    private static void AfterRewardTaken_Postfix(IRunState runState, Player player, Reward reward)
    {
        _ = HandleAfterRewardTakenAsync(runState, player, reward);
    }

    private static async Task HandleAfterRewardTakenAsync(IRunState runState, Player player, Reward reward)
    {
        _ = runState;
        _ = reward;
        if (!TryTakePendingPick(player, out int pick) || pick <= 0)
        {
            return;
        }

        await RemoveEnemyCardsOnPickup(player, pick);
    }

    private static async Task RemoveEnemyCardsOnPickup(Player player, int pick)
    {
        CardPile deckPile = PileType.Deck.GetPile(player);
        List<CardModel> enemyCards = deckPile.Cards
            .Where(c => c.Pool is EnemyCardPool)
            .ToList();

        if (enemyCards.Count == 0)
        {
            return;
        }

        int actualPick = enemyCards.Count < pick ? enemyCards.Count : pick;
        CardSelectorPrefs prefs = new(
            new LocString("cards", "COMICCHESS-RELEASE.selectionPrompt"),
            actualPick,
            actualPick);
        IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGrid(
            new BlockingPlayerChoiceContext(),
            enemyCards,
            player,
            prefs);

        foreach (CardModel selectedCard in selected.ToList())
        {
            await CardPileCmd.RemoveFromDeck(selectedCard);
        }
    }

    private static bool TryTakePendingPick(Player player, out int pick)
    {
        if (PendingPickByPlayer.TryGetValue(player, out pick))
        {
            PendingPickByPlayer.Remove(player);
            return true;
        }

        pick = 0;
        return false;
    }
}
