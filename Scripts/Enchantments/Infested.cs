using System.Linq;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.CardTags;

namespace ComicChess.TheQueen;

public sealed class Infested : QueenEnchantmentModel, ILearnIntentDamageBonusEnchantment
{
    /// <summary>
    /// 与原版 <see cref="MegaCrit.Sts2.Core.Models.EnchantmentModel.CanEnchant"/> 一致：已有同类型附魔时仍视为可附魔，
    /// 否则 <see cref="MegaCrit.Sts2.Core.Commands.CardCmd.Enchant"/> 的叠层与选牌筛选会依赖「仅 Lash」分支（见 <see cref="CanEnchant"/>）。
    /// </summary>
    public override bool IsStackable => true;

    public override bool ShowAmount => true;
    public override bool HasExtraCardText => false;

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card))
        {
            return false;
        }

        if (card is not LearnIntentCardModel)
        {
            return false;
        }

        if (card.Enchantment is not null)
        {
            return card.Enchantment is Infested && card is Lash;
        }

        return card.DynamicVars.Values.OfType<AmalgamLearnIntentDamageVar>().Any();
    }

    public decimal GetLearnIntentDamageBonus(CardModel card, AmalgamLearnIntentDamageVar var)
    {
        return Amount;
    }
}