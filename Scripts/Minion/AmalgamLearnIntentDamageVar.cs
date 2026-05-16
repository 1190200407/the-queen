using System.Globalization;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 卡面「学习进攻意图」伤害数字：用于<strong>展示</strong>，并走 <see cref="Hook.ModifyDamage"/>（出手方为聚合体）。
/// 写入灯槽的基数须与 <see cref="GetEffectiveFlatForOffenseIntent"/> 一致：卡牌 <see cref="DynamicVar.BaseValue"/> 加上
/// <see cref="ILearnIntentDamageBonusEnchantment"/> 的固定加成；之后再由 <see cref="Hook.ModifyDamage"/> 叠力量等，避免双算的是「力量」而非附魔层数。
/// </summary>
public sealed class AmalgamLearnIntentDamageVar : DamageVar
{
	public AmalgamLearnIntentDamageVar(decimal baseDamage, ValueProp props)
		: base("LearnIntentDamage", baseDamage, props)
	{
	}

	public AmalgamLearnIntentDamageVar(string name, decimal baseDamage, ValueProp props)
		: base(name, baseDamage, props)
	{
	}

	/// <summary>
	/// 学习进攻意图写入灯槽 / 与预览一致的「平砍基数」：<paramref name="damageVar"/> 的 <see cref="DynamicVar.BaseValue"/> 加上
	/// 当前牌附魔上实现的 <see cref="ILearnIntentDamageBonusEnchantment"/>（如感染）。
	/// </summary>
	public static decimal GetEffectiveFlatForOffenseIntent(CardModel card, AmalgamLearnIntentDamageVar damageVar)
	{
		decimal n = damageVar.BaseValue;
		if (card.Enchantment is ILearnIntentDamageBonusEnchantment bonus)
		{
			n += bonus.GetLearnIntentDamageBonus(card, damageVar);
		}

		return n;
	}

	/// <inheritdoc cref="GetEffectiveFlatForOffenseIntent(CardModel, AmalgamLearnIntentDamageVar)"/>
	public static decimal GetEffectiveFlatForOffenseIntent(CardModel card, string dynamicVarKey)
	{
		DynamicVar dv = card.DynamicVars[dynamicVarKey];
		if (dv is AmalgamLearnIntentDamageVar adv)
		{
			return GetEffectiveFlatForOffenseIntent(card, adv);
		}

		return dv.BaseValue;
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		decimal num = GetEffectiveFlatForOffenseIntent(card, this);
		EnchantmentModel? enchantment = card.Enchantment;
		if (enchantment != null)
		{
			if (!card.IsEnchantmentPreview)
			{
				EnchantedValue = num;
			}
		}

		// CardModel.CombatState 在部分牌堆/瞬间为 null；与 CalculatedDamageVar 一致用玩家生物上的战斗状态。
		Player? owner = card.Owner;
		CombatState? fromPile = card.CombatState;
		CombatState? fromOwnerCreature = owner?.Creature.CombatState;
		ICombatState? combatState = fromPile ?? fromOwnerCreature;
		// 与 NCard 一致：手牌/打出时 runGlobalHooks 为 true；弃牌堆等战斗内牌堆常为 false，但仍应显示当前战斗下的预览。
		bool inCombat = CombatManager.Instance is { IsInProgress: true };

		if (!(runGlobalHooks || inCombat) || owner is not Player p || combatState is not CombatState cs2)
		{
			PreviewValue = num;
			return;
		}

		Creature? amalgam = FriendlyAmalgamCmd.GetExisting(cs2, p);
		if (amalgam is { IsAlive: true })
		{
			// 与意图条一致：承伤者池随 AmalgamOffenseTargetingMode 变化（随机全体 / 全体各打 / 锁定单体）。
			Creature[] pool = AmalgamOffenseTargeting.GetPreviewReceiverPool(cs2, p);
			if (pool.Length > 0)
			{
				num = AmalgamIntentDamagePreview.PreviewOutgoingConsensusAmongReceivers(
					amalgam,
					pool,
					num,
					Props,
					cardSource: null,
					previewMode);
			}
		}

		PreviewValue = num;
	}

	/// <summary>非 <c>:diff()</c> 占位时与卡面数字一致（仍依赖先跑过 <see cref="UpdateCardPreview"/>）。</summary>
	public override string ToString() => ((int)PreviewValue).ToString(CultureInfo.InvariantCulture);
}
