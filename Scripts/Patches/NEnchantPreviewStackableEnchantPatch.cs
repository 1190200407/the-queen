using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 原版 <see cref="NEnchantPreview.Init"/> 对预览牌始终 <c>EnchantInternal</c>，会覆盖已有层数；
/// 可叠加附魔（如感染）实际走 <see cref="MegaCrit.Sts2.Core.Commands.CardCmd.Enchant"/> 的 <c>Amount +=</c>。
/// </summary>
internal sealed class NEnchantPreviewStackableEnchantPatch : IPatchMethod
{
	private static readonly FieldInfo BeforeField =
		AccessTools.Field(typeof(NEnchantPreview), "_before")!;

	private static readonly FieldInfo AfterField =
		AccessTools.Field(typeof(NEnchantPreview), "_after")!;

	private static readonly MethodInfo RemoveExistingCardsMethod =
		AccessTools.Method(typeof(NEnchantPreview), "RemoveExistingCards")!;

	public static string PatchId => "thequeen_nenchant_preview_stackable_enchant";
	public static string Description => "Stackable enchant preview adds to existing enchantment amount";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NEnchantPreview), nameof(NEnchantPreview.Init)),
	];

	public static bool Prefix(
		NEnchantPreview __instance,
		CardModel card,
		EnchantmentModel canonicalEnchantment,
		int amount)
	{
		if (!ShouldStackPreviewEnchant(card, canonicalEnchantment))
		{
			return true;
		}

		canonicalEnchantment.AssertCanonical();
		RemoveExistingCardsMethod.Invoke(__instance, null);

		var before = (Control)BeforeField.GetValue(__instance)!;
		NPreviewCardHolder beforeHolder = NPreviewCardHolder.Create(
			NCard.Create(card),
			showHoverTips: true,
			scaleOnHover: false);
		before.AddChildSafely(beforeHolder);
		beforeHolder.CardNode.UpdateVisuals(card.Pile?.Type ?? PileType.None, CardPreviewMode.Normal);

		CardModel afterCard = card.CardScope.CloneCard(card);
		afterCard.IsEnchantmentPreview = true;
		afterCard.Enchantment!.Amount += amount;
		afterCard.FinalizeUpgradeInternal();

		var after = (Control)AfterField.GetValue(__instance)!;
		NPreviewCardHolder afterHolder = NPreviewCardHolder.Create(
			NCard.Create(afterCard),
			showHoverTips: true,
			scaleOnHover: false);
		after.AddChildSafely(afterHolder);
		afterHolder.CardNode.UpdateVisuals(PileType.None, CardPreviewMode.Normal);

		return false;
	}

	private static bool ShouldStackPreviewEnchant(CardModel card, EnchantmentModel canonicalEnchantment)
	{
		if (!canonicalEnchantment.IsStackable)
		{
			return false;
		}

		EnchantmentModel? existing = card.Enchantment;
		return existing != null && existing.GetType() == canonicalEnchantment.GetType();
	}
}
