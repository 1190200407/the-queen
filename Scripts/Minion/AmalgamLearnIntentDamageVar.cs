using System.Globalization;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 卡面「学习进攻意图」伤害数字：仅用于<strong>展示</strong>，走 <see cref="Hook.ModifyDamage"/>（出手方为聚合体）。
/// 灯槽内 <see cref="AmalgamOffenseIntentAction"/> 的 <see cref="AmalgamActionModel.Amount"/> 须保持<strong>原始基础值</strong>，由结算再叠力量，避免双算。
/// </summary>
public sealed class AmalgamLearnIntentDamageVar : DamageVar
{
	public AmalgamLearnIntentDamageVar(decimal baseDamage, ValueProp props)
		: base("LearnIntentDamage", baseDamage, props)
	{
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		decimal num = BaseValue;
		EnchantmentModel? enchantment = card.Enchantment;
		if (enchantment != null)
		{
			num += enchantment.EnchantDamageAdditive(num, Props);
			num *= enchantment.EnchantDamageMultiplicative(num, Props);
			if (!card.IsEnchantmentPreview)
			{
				EnchantedValue = num;
			}
		}

		// CardModel.CombatState 在部分牌堆/瞬间为 null；与 CalculatedDamageVar 一致用玩家生物上的战斗状态。
		Player? owner = card.Owner;
		CombatState? fromPile = card.CombatState;
		CombatState? fromOwnerCreature = owner?.Creature.CombatState;
		CombatState? combatState = fromPile ?? fromOwnerCreature;
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
			// 与意图条一致：随机打存活敌人 — 仅当各敌预览取整后相同时才显示修正值，否则保持基础值（不自指牌的 target 当承伤者）。
			Creature[] pool = cs2.Enemies.Where(e => e.IsAlive).ToArray();
			decimal afterHook = AmalgamIntentDamagePreview.PreviewOutgoingConsensusAmongReceivers(
				amalgam,
				pool,
				BaseValue,
				Props,
				cardSource: null,
				previewMode);

			num = afterHook;
		}

		PreviewValue = num;
	}

	/// <summary>非 <c>:diff()</c> 占位时与卡面数字一致（仍依赖先跑过 <see cref="UpdateCardPreview"/>）。</summary>
	public override string ToString() => ((int)PreviewValue).ToString(CultureInfo.InvariantCulture);
}
