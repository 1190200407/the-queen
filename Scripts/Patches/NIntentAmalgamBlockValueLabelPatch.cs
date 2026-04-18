using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.addons.mega_text;

namespace ComicChess.TheQueen;

/// <summary>
/// 原版 <see cref="NIntent"/> 仅在 <c>AttackIntent</c> / <c>StatusIntent</c> 时填充 <c>%Value</c>；
/// <see cref="AmalgamGainBlockIntent"/> 为 <c>AbstractIntent</c>，需在此处补写格挡数字。
/// </summary>
[HarmonyPatch(typeof(NIntent), "UpdateVisuals")]
public static class NIntentAmalgamBlockValueLabelPatch
{
	[HarmonyPostfix]
	private static void Postfix(NIntent __instance)
	{
		var traverse = Traverse.Create(__instance);
		AbstractIntent? intent = traverse.Field<AbstractIntent>("_intent").Value;
		IEnumerable<Creature>? targets = traverse.Field<IEnumerable<Creature>>("_targets").Value;
		Creature? owner = traverse.Field<Creature>("_owner").Value;
		MegaRichTextLabel? valueLabel = traverse.Field<MegaRichTextLabel>("_valueLabel").Value;

		if (intent is not AmalgamGainBlockIntent || valueLabel == null || targets == null || owner == null)
		{
			return;
		}

		valueLabel.Text = intent.GetIntentLabel(targets, owner).GetFormattedText() ?? "";
	}
}
