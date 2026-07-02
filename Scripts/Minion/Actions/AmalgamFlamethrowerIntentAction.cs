using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>一个意图：抽牌并附魔（灼烧），并在下回合开始造成伤害（以能力实现）�?/summary>
public sealed class AmalgamFlamethrowerIntentAction : AmalgamActionModel
{
    public override string Key => "flamethrower";

    private readonly decimal _draw;
    private readonly decimal _enchantAmount;
    private readonly decimal _nextTurnDamage;

    public AmalgamFlamethrowerIntentAction(decimal draw, decimal enchantAmount, decimal nextTurnDamage)
    {
        _draw = draw;
        _enchantAmount = enchantAmount;
        _nextTurnDamage = nextTurnDamage;
    }

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_FLAMETHROWER",
            _ => Task.CompletedTask,
            new AmalgamDrawAndEnchantWithAmountIntent<Burn>(_draw, _enchantAmount),
            new AmalgamGainBuffIntent("AMALGAM_NEXT_ROUND_ATTACK_POWER", _nextTurnDamage));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        if (amalgam.PetOwner is not Player queen || queen.Creature is not { IsAlive: true } queenCreature)
        {
            return;
        }

        if (_draw > 0m)
        {
            IEnumerable<CardModel> drawn = await CardPileCmd.Draw(choiceContext, _draw, queen);
            foreach (CardModel card in drawn)
            {
                EnchantmentModel burn = ModelDb.Enchantment<Burn>().ToMutable();
                if (!burn.CanEnchant(card))
                {
                    continue;
                }

                CardCmd.Enchant(burn, card, amount: _enchantAmount);
            }
        }

        if (_nextTurnDamage > 0m)
        {
            await PowerCmd.Apply<AmalgamNextRoundAttackPower>(amalgam, _nextTurnDamage, applier: queenCreature, cardSource: null, silent: true);
        }
    }
}

