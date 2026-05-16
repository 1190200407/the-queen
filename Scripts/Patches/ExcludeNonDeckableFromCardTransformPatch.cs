using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class ExcludeNonDeckableFromCardTransformPatch : IPatchMethod
{
	public static string PatchId => "thequeen_exclude_non_deckable_transform";
	public static string Description => "Filter transform options to deckable cards only";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CardFactory), "GetFilteredTransformationOptions", new[]
		{
			typeof(CardModel),
			typeof(IEnumerable<CardModel>),
			typeof(bool),
		}),
	];

	public static void Postfix(ref CardModel[] __result)
	{
		if (__result is not { Length: > 0 })
		{
			return;
		}

		CardModel[] without = __result.Where(static c => c.ShouldAddToDeck(c)).ToArray();
		if (without.Length > 0)
		{
			__result = without;
		}
	}
}
