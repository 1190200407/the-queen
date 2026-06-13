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

/// <summary>聚合体意图：抽牌后为抽到的牌附魔指定层数（若可附魔）�?/summary>
public sealed class AmalgamDrawAndEnchantWithAmountIntentAction<TEnchantment> : AmalgamActionModel
    where TEnchantment : EnchantmentModel
{
    public override string Key => GenericPoolKey("draw_and_enchant_with_amount", typeof(TEnchantment));

    private decimal _enchantAmount;

    public static readonly float CastAnimDelay = 1.5f;

    public AmalgamDrawAndEnchantWithAmountIntentAction()
    {
    }

    public AmalgamDrawAndEnchantWithAmountIntentAction(decimal drawCount, decimal enchantAmount)
        : this()
    {
        Amount = drawCount;
        _enchantAmount = enchantAmount;
    }

    protected override void ResetForInit()
    {
        base.ResetForInit();
        _enchantAmount = 0m;
    }

    public override bool Init(decimal amount) => false;

    public override bool Init(object[] args)
    {
        if (!AmalgamActionArgs.TryGetDecimal(args, 0, out decimal drawCount)
            || !AmalgamActionArgs.TryGetDecimal(args, 1, out decimal enchantAmount)
            || !AmalgamActionArgs.IsPositive(drawCount)
            || !AmalgamActionArgs.IsPositive(enchantAmount))
        {
            return false;
        }

        ResetForInit();
        Amount = drawCount;
        _enchantAmount = enchantAmount;
        return true;
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
