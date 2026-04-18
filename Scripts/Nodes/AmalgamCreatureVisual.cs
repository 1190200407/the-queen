using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace ComicChess.TheQueen;

/// <summary>火炬头聚合体：挂载 <see cref="IntentSlotManager"/> 与三格意图槽。</summary>
[GlobalClass]
public partial class AmalgamCreatureVisual : NCreatureVisuals
{
	public override void _Ready()
	{
		base._Ready();
		Callable.From(MountIntentSlotManager).CallDeferred();
	}

	private void MountIntentSlotManager()
	{
		if (GetParent() is not NCreature creature)
		{
			Log.Warn($"[AmalgamCreatureVisual] parent is not NCreature: {GetParent()?.GetType().Name ?? "null"}");
			return;
		}

		IntentSlotManager.MountOnCreature(creature, this);
	}
}
