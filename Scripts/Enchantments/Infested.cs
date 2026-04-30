using System.Linq;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ComicChess.TheQueen;

public sealed class Infested : QueenEnchantmentModel, ILearnIntentDamageBonusEnchantment
{
    public override bool ShowAmount => true;
    public override bool HasExtraCardText => false;

    public override bool CanEnchant(CardModel card)
    {
        if (card is not LearnIntentCardModel)
        {
            return false;
        }
        if (card.Enchantment is not null && card is not Lash)
        {
            return false;
        }
        // 有攻击意图的牌
        return card.DynamicVars.Values.OfType<AmalgamLearnIntentDamageVar>().Any();
    }

    public decimal GetLearnIntentDamageBonus(CardModel card, AmalgamLearnIntentDamageVar var)
    {
        return Amount;
    }
}