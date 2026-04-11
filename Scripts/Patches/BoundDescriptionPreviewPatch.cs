using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;

namespace ComicChess.TheQueen;

[HarmonyPatch]
internal static class BoundDescriptionPreviewPatch
{
	private static string? GetBoundPreviewLine()
	{
		return new LocString("cards", "COMICCHESS-BOUNDED.description").GetFormattedText();
	}

	/// <summary>
	/// 拦截私有实现，覆盖升级预览、各牌堆描述等所有出口，避免漏补丁。
	/// <see cref="CardModel"/> 内 <c>DescriptionPreviewType</c> 为 private enum，需反射取类型。
	/// </summary>
	[HarmonyTargetMethod]
	private static MethodBase TargetMethod()
	{
		Type? previewType = typeof(CardModel).GetNestedType("DescriptionPreviewType", BindingFlags.NonPublic);
		if (previewType == null)
		{
			throw new System.InvalidOperationException("CardModel.DescriptionPreviewType nested type not found");
		}

		MethodInfo? m = AccessTools.Method(typeof(CardModel), "GetDescriptionForPile", [typeof(PileType), previewType, typeof(Creature)]);
		return m ?? throw new System.InvalidOperationException("CardModel.GetDescriptionForPile(PileType, DescriptionPreviewType, Creature) not found");
	}

	[HarmonyPostfix]
	private static void GetDescriptionForPileCore_Postfix(CardModel __instance, ref string __result)
	{
		TryAppendBoundPreviewText(__instance, ref __result);
	}

	private static void TryAppendBoundPreviewText(CardModel card, ref string description)
	{
		// 与 UseBoundAfflictionOverlayForPreview 一致：自带魂缚的牌在 cards.json 勿重复写魂缚行。
		if (card is not QueenCardModel queen || !queen.UseBoundAfflictionOverlayForPreview)
		{
			return;
		}

		if (card.Affliction != null)
		{
			return;
		}

		string? line = GetBoundPreviewLine();
		if (string.IsNullOrWhiteSpace(line))
		{
			return;
		}
		if (description.Contains(line))
		{
			return;
		}

		description = string.IsNullOrEmpty(description) ? line : $"{description}\n{line}";
	}
}
