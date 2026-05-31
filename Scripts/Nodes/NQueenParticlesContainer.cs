using System.Reflection;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace ComicChess.TheQueen;

/// <summary>
/// Mod 工程内挂载用，继承原版 <see cref="NParticlesContainer"/>，行为完全一致。
/// Ritsu 保留 <see cref="NEnergyCounter"/> 子类场景时不会走工厂里的 <c>SetParticles</c>，
/// 且 mod 场景导出常缺少 <c>_particles</c> 绑定，因此在进入树时自动收集子级 <see cref="GpuParticles2D"/> 并同步到基类字段。
/// </summary>
[GlobalClass]
public partial class NQueenParticlesContainer : NParticlesContainer
{
	private static readonly FieldInfo BaseParticlesField =
		typeof(NParticlesContainer).GetField("_particles", BindingFlags.Instance | BindingFlags.NonPublic)!;

	// Godot C# 无法把场景导出写进基类的 private [Export]，子类再声明一份供编辑器绑定。
	[Export]
	private Array<GpuParticles2D>? _particles;

	public override void _EnterTree()
	{
		SyncParticlesToBase();
		base._EnterTree();
	}

	public override void _Ready()
	{
		SyncParticlesToBase();
		base._Ready();
	}

	private void SyncParticlesToBase()
	{
		Array<GpuParticles2D> particles = _particles ?? [];
		if (particles.Count == 0)
		{
			particles = [];
			CollectGpuParticles(this, particles);
			_particles = particles;
		}

		BaseParticlesField.SetValue(this, particles);
	}

	private static void CollectGpuParticles(Node node, Array<GpuParticles2D> particles)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is GpuParticles2D gpuParticle)
			{
				particles.Add(gpuParticle);
			}

			CollectGpuParticles(child, particles);
		}
	}
}
