using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 燃烧木棍：仅在手牌已满、复制牌无法入手时会改道弃牌堆（消逝再进消耗）并重入
/// <see cref="MegaCrit.Sts2.Core.Hooks.Hook.AfterCardExhausted"/> 时，提前标记 <c>WasUsedThisCombat</c>。
/// 手牌未满时走原版逻辑，不影响其他 mod 对同一方法的补丁。
/// </summary>
internal sealed class BurningSticksAfterCardExhaustedPatch : IPatchMethod
{
	private static readonly FieldInfo WasUsedThisCombatField = typeof(BurningSticks).GetField(
		"_wasUsedThisCombat",
		BindingFlags.Instance | BindingFlags.NonPublic)!;

	public static string PatchId => "thequeen_burning_sticks_after_card_exhausted";
	public static string Description => "Burning Sticks: early WasUsedThisCombat only when hand is full";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(BurningSticks), nameof(BurningSticks.AfterCardExhausted)),
	];

	public static bool Prefix(
		BurningSticks __instance,
		PlayerChoiceContext choiceContext,
		CardModel card,
		bool causedByEthereal,
		ref Task __result)
	{
		if (!ShouldUseHandFullEarlyClaimPath(__instance, card))
		{
			return true;
		}

		_ = choiceContext;
		_ = causedByEthereal;
		__result = AfterCardExhaustedHandFullImpl(__instance, card);
		return false;
	}

	private static bool ShouldUseHandFullEarlyClaimPath(BurningSticks relic, CardModel card)
	{
		if (card.Owner != relic.Owner)
		{
			return false;
		}

		if ((bool)WasUsedThisCombatField.GetValue(relic)!)
		{
			return false;
		}

		if (card.Type != CardType.Skill)
		{
			return false;
		}

		CardPile hand = PileType.Hand.GetPile(relic.Owner);
		return hand.Cards.Count >= CardPile.MaxCardsInHand;
	}

	private static async Task AfterCardExhaustedHandFullImpl(BurningSticks relic, CardModel card)
	{
		relic.Flash();
		WasUsedThisCombatField.SetValue(relic, true);
		relic.Status = RelicStatus.Normal;

		CardModel clone = card.CreateClone();
		await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Hand, relic.Owner);
	}
}
