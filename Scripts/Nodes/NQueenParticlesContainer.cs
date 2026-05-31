using System.Reflection;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace ComicChess.TheQueen;

/// <summary>
/// Mod 工程内挂载用，继承原版 <see cref="NParticlesContainer"/>。
/// 基类 <see cref="NParticlesContainer.Restart"/> 只读其 private <c>_particles</c> 字段；
/// 子类必须在运行时把粒子列表写进去（场景导出或从子节点收集）。
/// </summary>
[GlobalClass]
public partial class NQueenParticlesContainer : NParticlesContainer
{
	private static readonly FieldInfo BaseParticlesField =
		typeof(NParticlesContainer).GetField("_particles", BindingFlags.Instance | BindingFlags.NonPublic)!;

	[Export]
	public Array<GpuParticles2D>? _particles;

	public override void _EnterTree()
	{
		SyncBaseParticles();
		base._EnterTree();
	}

	public override void _Ready()
	{
		SyncBaseParticles();
		base._Ready();
	}

	private void SyncBaseParticles()
	{
		Array<GpuParticles2D> resolved = ResolveParticles();
		_particles = resolved;
		BaseParticlesField.SetValue(this, resolved);
	}

	private Array<GpuParticles2D> ResolveParticles()
	{
		if (_particles is { Count: > 0 })
		{
			return _particles;
		}

		Array<GpuParticles2D> collected = [];
		CollectParticles(this, collected);
		return collected;
	}

	private static void CollectParticles(Node node, Array<GpuParticles2D> particles)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is GpuParticles2D gpuParticles)
			{
				particles.Add(gpuParticles);
			}

			CollectParticles(child, particles);
		}
	}
}
