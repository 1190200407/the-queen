using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：抽牌后为抽到的牌附魔指定层数（若可附魔）。</summary>
public sealed class AmalgamDrawAndEnchantWithAmountIntentAction<TEnchantment> : AmalgamActionModel
    where TEnchantment : EnchantmentModel
{
    private const string EnchantAmountParam = "enchantAmount";

    private readonly decimal _enchantAmount;

    public static readonly float CastAnimDelay = 1.5f;

    public AmalgamDrawAndEnchantWithAmountIntentAction(decimal drawCount, decimal enchantAmount)
        : base(new Dictionary<string, decimal>
        {
            [AmountParam] = drawCount,
            [EnchantAmountParam] = enchantAmount,
        })
    {
        _enchantAmount = GetParameterOrDefault(EnchantAmountParam, 0m);
    }

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            $"AMALGAM_INTENT_DRAW_ENCHANT_AMOUNT_{typeof(TEnchantment).Name}",
            _ => Task.CompletedTask,
            new AmalgamDrawAndEnchantWithAmountIntent<TEnchantment>(Amount, _enchantAmount));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        if (amalgam.PetOwner is not { Creature: { } ownerCreature } || !ownerCreature.IsAlive)
        {
            return;
        }

        Player? owner = ownerCreature.Player;
        if (owner == null)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Buff", CastAnimDelay);

        IEnumerable<CardModel> drawn = await CardPileCmd.Draw(choiceContext, Amount, owner);
        foreach (CardModel card in drawn)
        {
            EnchantmentModel enchantment = ModelDb.Enchantment<TEnchantment>().ToMutable();
            Log.Info($"card: {card.Title} can enchant: {enchantment.CanEnchant(card)}");
            if (!enchantment.CanEnchant(card))
            {
                continue;
            }

            CardCmd.Enchant(enchantment, card, amount: _enchantAmount);
        }
    }
}
