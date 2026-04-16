using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MinionLib.Layout;

namespace ComicChess.TheQueen;

/// <summary>
/// 在默认布局之后，仅把 <see cref="FriendlyAmalgam"/> 的节点再平移固定像素（不改动 Front 槽位逻辑本身）。
/// </summary>
internal sealed class FriendlyAmalgamLayoutOffset : IMinionLayout
{
	private static readonly Vector2 ScreenOffset = new(120f, -75f);

	public bool IsActive => true;

	public void ApplyLayout(MinionLayoutContext context)
	{
		foreach (NCreature node in context.Positions.Keys.ToList())
		{
			if (node.Entity.Monster is not FriendlyAmalgam)
			{
				continue;
			}

			context.Positions[node] += ScreenOffset;
		}
	}
}
