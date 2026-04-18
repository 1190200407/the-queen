using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// <see cref="AmalgamDieForYouPower.ModifyHpLostAfterOsty"/> 截断承伤后，需把溢出伤害再打给主人；原版 <see cref="CreatureCmd.Damage"/> 仅处理 Overkill 分流。
/// </summary>
[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), typeof(PlayerChoiceContext), typeof(IEnumerable<Creature>), typeof(decimal), typeof(ValueProp), typeof(Creature), typeof(CardModel))]
public static class AmalgamBodyguardSpillDamagePatch
{
	[HarmonyPostfix]
	private static async Task<IEnumerable<DamageResult>> Postfix(
		Task<IEnumerable<DamageResult>> __result,
		PlayerChoiceContext choiceContext,
		IEnumerable<Creature> targets,
		decimal amount,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource)
	{
		_ = targets;
		_ = amount;
		List<DamageResult> results = (await __result).ToList();
		foreach (DamageResult dr in results)
		{
			Creature receiver = dr.Receiver;
			if (receiver.Monster is not FriendlyAmalgam || receiver.PetOwner is not { Creature: { IsAlive: true } owner })
			{
				continue;
			}

			AmalgamDieForYouPower? power = receiver.GetPower<AmalgamDieForYouPower>();
			if (power == null)
			{
				continue;
			}

			decimal spill = power.ConsumeSpillAndClearRedirect();
			if (spill <= 0m || !owner.IsAlive)
			{
				continue;
			}

			AmalgamDieForYouPower.SuppressBodyguardRedirect = true;
			try
			{
				await CreatureCmd.Damage(choiceContext, owner, spill, props, dealer, cardSource);
			}
			finally
			{
				AmalgamDieForYouPower.SuppressBodyguardRedirect = false;
			}
		}

		return results;
	}
}
