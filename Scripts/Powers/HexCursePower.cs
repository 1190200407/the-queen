using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace ComicChess.TheQueen;

/// <summary>厄咒之力：每消耗 3 张带虚无的牌，将 1 张随机颜色牌加入手牌。</summary>
public sealed class HexCursePower : QueenPowerModel
{
    private const int Threshold = 1;

    private sealed class Data
    {
        public int EtherealExhaustedSinceLastTrigger;
    }

    protected override object? InitInternalData() => new Data();

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override int DisplayAmount => Threshold - GetInternalData<Data>().EtherealExhaustedSinceLastTrigger;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Threshold", Threshold)];

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (!causedByEthereal || card.Owner?.Creature != base.Owner)
        {
            return;
        }

        Player? player = base.Owner.Player;
        if (player == null)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        data.EtherealExhaustedSinceLastTrigger++;
        InvokeDisplayAmountChanged();

        if (data.EtherealExhaustedSinceLastTrigger < Threshold)
        {
            return;
        }

        data.EtherealExhaustedSinceLastTrigger = 0;
        InvokeDisplayAmountChanged();
        Flash();

        List<CardModel> allUnlocked = player.UnlockState.CharacterCardPools
            .SelectMany(p => p.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint))
            .Where(static c => c.Rarity is not (CardRarity.Basic or CardRarity.Ancient))
            .ToList();

        if (allUnlocked.Count == 0)
        {
            return;
        }

        Rng rng = player.RunState.Rng.CombatCardGeneration;
        CardModel random = CardFactory.GetDistinctForCombat(player, allUnlocked, 1, rng).First();
        await CardPileCmd.AddGeneratedCardToCombat(random, PileType.Hand, player);
    }
}

