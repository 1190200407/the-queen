using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.addons.mega_text;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class NIntentAmalgamBlockValueLabelPatch : IPatchMethod
{
	public static string PatchId => "thequeen_nintent_amalgam_block_label";
	public static string Description => "Fill intent value label for AmalgamGainBlockIntent";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NIntent), "UpdateVisuals"),
	];

	public static void Postfix(NIntent __instance)
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
