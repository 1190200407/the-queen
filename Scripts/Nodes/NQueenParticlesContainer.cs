using System.Reflection;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace ComicChess.TheQueen;

/// <summary>
/// Mod 工程内挂载用，继承原版 <see cref="NParticlesContainer"/>，行为完全一致。
/// <see cref="NEnergyCounter"/> 通过 <see cref="NParticlesContainer"/> 类型引用调用 <c>Restart()</c> / <c>SetEmitting()</c>，
/// 因此必须继承原版类，而不是复制成独立的 <see cref="Godot.Node2D"/>。
/// </summary>
[GlobalClass]
public partial class NQueenParticlesContainer : NParticlesContainer
{
	private static readonly FieldInfo BaseParticlesField =
		typeof(NParticlesContainer).GetField("_particles", BindingFlags.Instance | BindingFlags.NonPublic)!;

	// Godot C# 无法把场景导出写进基类的 private [Export]，子类必须再声明一份并在 _Ready 同步。
	[Export]
	private Array<GpuParticles2D>? _particles;

	public override void _Ready()
	{
		BaseParticlesField.SetValue(this, _particles);
		base._Ready();
	}
}
