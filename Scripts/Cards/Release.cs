using System.Collections.Generic;
using System.Threading.Tasks;

using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>�?��??�?�?�起�?��??�?�并移�?��??�?�坡�??�?�?�坡丝�?��?��??�?�??/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class Release : QueenCardModel
{
    private const int energyCost = -1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public Release()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override bool ShouldAddToDeck(CardModel card)
    {
        return card is not Release;
    }

    public override async Task AfterAddToDeckPrevented(CardModel card)
    {
        // ??????????????????????????????????????? SelectCard?
        // ??????????? NCardRewardSelectionScreenSelectCardGuardPatch ???? TCS ?? SetResult?
        SceneTree? tree = NGame.Instance?.GetTree();
        if (tree != null)
        {
            await NGame.Instance!.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        }

        await RemoveEnemyCardsOnPickup(card.Owner, card.IsUpgraded ? 2 : 1);
    }

    private async Task RemoveEnemyCardsOnPickup(Player player, int pick)
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
            new LocString("cards", "THE_QUEEN_CARD_RELEASE.selectionPrompt"),
            0,
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

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Task.CompletedTask;
}
