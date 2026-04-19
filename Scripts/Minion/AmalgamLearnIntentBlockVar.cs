using System.Collections.Generic;
using System.Globalization;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 卡面「学习格挡意图」数字：仅用于<strong>展示</strong>，走 <see cref="Hook.ModifyBlock"/>（目标为玩家生物）。
/// 不继承 <see cref="BlockVar"/>，避免 <see cref="BlockVar"/> 预览链路里对<strong>敏捷</strong>的叠算；灯槽内 <see cref="AmalgamGainBlockIntentAction"/> 的 <see cref="AmalgamActionModel.Amount"/> 仍为原始基数，由 <see cref="CreatureCmd.GainBlock"/> 结算时再叠敏捷等。
/// </summary>
public sealed class AmalgamLearnIntentBlockVar : DynamicVar
{
	private readonly ValueProp _props;

	public AmalgamLearnIntentBlockVar(decimal baseBlock, ValueProp props)
		: base("LearnIntentBlock", baseBlock)
	{
		_props = props;
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		decimal num = BaseValue;
		EnchantmentModel? enchantment = card.Enchantment;
		if (enchantment != null)
		{
			num += enchantment.EnchantBlockAdditive(num, _props);
			num *= enchantment.EnchantBlockMultiplicative(num, _props);
			if (!card.IsEnchantmentPreview)
			{
				EnchantedValue = num;
			}
		}

		Player? owner = card.Owner;
		CombatState? fromPile = card.CombatState;
		CombatState? fromOwnerCreature = owner?.Creature.CombatState;
		CombatState? combatState = fromPile ?? fromOwnerCreature;
		bool inCombat = CombatManager.Instance is { IsInProgress: true };

		if (!(runGlobalHooks || inCombat) || owner is not Player p || combatState is not CombatState cs2)
		{
			PreviewValue = num;
			return;
		}

		num = Hook.ModifyBlock(cs2, p.Creature, num, _props, card, null, out IEnumerable<AbstractModel> _);
		PreviewValue = num;
	}

	public override string ToString() => ((int)PreviewValue).ToString(CultureInfo.InvariantCulture);
}
