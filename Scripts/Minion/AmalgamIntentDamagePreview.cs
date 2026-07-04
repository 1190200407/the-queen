using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>聚合体作为伤害来源时，与 <see cref="CreatureCmd.Damage"/> 一致的 <see cref="Hook.ModifyDamage"/> 预览。</summary>
public static class AmalgamIntentDamagePreview
{
	/// <summary>
	/// <see cref="Hook.ModifyDamage"/> 的 <c>target</c> 为<strong>即将承伤的一方</strong>（敌人）。
	/// 卡面 <see cref="CardModel.UpdateCardPreview"/> 传入的 <paramref name="cardContextTarget"/> 常是「这张牌的目标」，
	/// 自指牌（如 <c>TargetType.Self</c>）会是<strong>玩家</strong>，若直接当作承伤者会把<strong>玩家身上的易伤</strong>误算进预览。
	/// </summary>
	public static Creature? ResolveDamageReceiverForPetAttackPreview(
		ICombatState combatState,
		Creature? cardContextTarget)
	{
		if (cardContextTarget is { Side: CombatSide.Enemy })
		{
			return cardContextTarget;
		}

		return combatState.HittableEnemies.FirstOrDefault();
	}

	public static decimal PreviewOutgoing(
		Creature amalgamDealer,
		Creature? damageTarget,
		decimal baseDamage,
		ValueProp props,
		CardModel? cardSource,
		CardPreviewMode previewMode)
	{
		ICombatState? combatState = amalgamDealer.CombatState;
		IRunState runState = IRunState.GetFrom(new[] { amalgamDealer });
		return Hook.ModifyDamage(
			runState,
			combatState,
			damageTarget,
			amalgamDealer,
			baseDamage,
			props,
			cardSource,
			null,
			ModifyDamageHookType.All,
			previewMode,
			out IEnumerable<AbstractModel> _);
	}

	/// <summary>
	/// 与原版 <see cref="Hook.ModifyDamage"/> 在手牌「多目标 / 随机敌」预览时的规则一致：
	/// 对每个承伤者各跑一遍 <see cref="PreviewOutgoing"/>；若取整后伤害<strong>全员相同</strong>则返回该值，
	/// 否则返回 <paramref name="baseDamage"/>（不代入任一敌人的易伤等分歧）。
	/// 候选列表须与真实结算选敌范围一致（聚合体进攻意图为 <c>Enemies</c> 中仍存活者）。
	/// </summary>
	public static decimal PreviewOutgoingConsensusAmongReceivers(
		Creature amalgamDealer,
		IReadOnlyList<Creature> damageReceivers,
		decimal baseDamage,
		ValueProp props,
		CardModel? cardSource,
		CardPreviewMode previewMode)
	{
		if (damageReceivers.Count == 0)
		{
			return baseDamage;
		}

		if (damageReceivers.Count == 1)
		{
			return PreviewOutgoing(amalgamDealer, damageReceivers[0], baseDamage, props, cardSource, previewMode);
		}

		decimal first = PreviewOutgoing(amalgamDealer, damageReceivers[0], baseDamage, props, cardSource, previewMode);
		for (int i = 1; i < damageReceivers.Count; i++)
		{
			decimal d = PreviewOutgoing(amalgamDealer, damageReceivers[i], baseDamage, props, cardSource, previewMode);
			if ((int)d != (int)first)
			{
				return baseDamage;
			}
		}

		return first;
	}
}
