using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ComicChess.TheQueen;

/// <summary>
/// 标记类接口：该附魔会提升「学习意图伤害」的展示数值（仅作用于 <see cref="AmalgamLearnIntentDamageVar"/>）。
/// </summary>
public interface ILearnIntentDamageBonusEnchantment
{
    decimal GetLearnIntentDamageBonus(CardModel card, AmalgamLearnIntentDamageVar var);
}

