using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace ComicChess.TheQueen;

/// <summary>学习意图时，在聚合体身上弹出与获得能力同款的绿色飘字。</summary>
internal static class LearnIntentAppliedVfx
{
	private static readonly LocString Title = new("static_hover_tips", "learn_intent.vfx_title");

	public static void Play(Creature amalgamCreature)
	{
		NLearnIntentAppliedVfx? vfx = NLearnIntentAppliedVfx.Create(amalgamCreature, Title.GetFormattedText());
		if (vfx == null)
		{
			return;
		}

		NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(amalgamCreature);
		NPowerAppliedBuffVfx? buffVfx = creatureNode == null
			? null
			: NPowerAppliedBuffVfx.Create(creatureNode.PowerAppliedVfxSpawnPosition);

		Callable.From(() =>
		{
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
			if (buffVfx != null)
			{
				NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(buffVfx);
			}
		}).CallDeferred();

		SfxCmd.Play("event:/sfx/buff");
	}
}
