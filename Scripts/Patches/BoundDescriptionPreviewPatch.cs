using System;
using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class BoundDescriptionPreviewPatch : IPatchMethod
{
	private static readonly ModPatchTarget[] Targets = BuildTargets();

	public static string PatchId => "thequeen_bound_description_preview";
	public static string Description => "Append bound affliction preview line on queen self-bound cards";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() => Targets;

	private static ModPatchTarget[] BuildTargets()
	{
		Type? previewType = typeof(CardModel).GetNestedType("DescriptionPreviewType", BindingFlags.NonPublic);
		if (previewType == null)
		{
			throw new InvalidOperationException("CardModel.DescriptionPreviewType nested type not found");
		}

		return
		[
			new(typeof(CardModel), "GetDescriptionForPile",
				new[] { typeof(PileType), previewType, typeof(Creature) }),
		];
	}

	public static void Postfix(CardModel __instance, ref string __result)
	{
		TryAppendBoundPreviewText(__instance, ref __result);
	}

	private static string? GetBoundPreviewLine() =>
		new LocString("cards", "COMICCHESS-BOUNDED.description").GetFormattedText();

	private static void TryAppendBoundPreviewText(CardModel card, ref string description)
	{
		bool previewBound =
			(card is QueenCardModel queen && queen.HasSelfBound) ||
			card.Enchantment is SoulLight;

		if (!previewBound)
		{
			return;
		}

		if (card.CombatState != null)
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
