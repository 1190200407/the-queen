using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// PersonalHivePower：与原版奥斯提一致，聚合体作为出手者时将“伤害归因/处理对象”转移给主人。
/// 只影响 <see cref="PersonalHivePower.AfterDamageReceived"/> 内部看到的参数，不改实际伤害结算与表现。
/// </summary>
/// <remarks>
/// sts2.dll 中方法签名为
/// <c>Task AfterDamageReceived(PlayerChoiceContext, Creature, DamageResult _, ValueProp, Creature dealer, CardModel cardSource)</c>，
/// 其中 <c>dealer</c> / <c>cardSource</c> 带 <c>[Nullable(2)]</c>，等价于 C# 的 <c>Creature?</c> / <c>CardModel?</c>。
/// 第三个参数名为 <c>_</c>，前缀里用 <see cref="HarmonyArgumentAttribute"/> 绑定到其它合法标识符（如 <c>damageResult</c>）；其余形参名须与原版一致，否则 Harmony 会报 “Parameter … not found”。
/// </remarks>
[HarmonyPatch(typeof(PersonalHivePower), nameof(PersonalHivePower.AfterDamageReceived))]
internal static class PersonalHivePowerAmalgamDealerTransferPatch
{
	[HarmonyPrefix]
	private static bool Prefix(
		PersonalHivePower __instance,
		PlayerChoiceContext choiceContext,
		Creature target,
		[HarmonyArgument("_")] DamageResult damageResult,
		ValueProp props,
		ref Creature? dealer,
		CardModel? cardSource,
		ref Task __result)
	{
		_ = __instance;
		_ = choiceContext;
		_ = target;
		_ = damageResult;
		_ = props;
		_ = cardSource;

		if (dealer?.Monster is FriendlyAmalgam)
		{
			Creature? ownerCreature = dealer.PetOwner?.Creature;
			if (ownerCreature != null)
			{
				dealer = ownerCreature;
			}
		}

		// 原版此处会访问 dealer.Player；无玩家且非友方聚合体时跳过本能力逻辑（async 前缀需补全 Task）。
		if (dealer != null && dealer.Player is null)
		{
			__result = Task.CompletedTask;
			return false;
		}

		return true;
	}
}
