using Godot;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace ComicChess.TheQueen;

/// <summary>
/// Mod 工程内挂载用，继承原版 <see cref="NParticlesContainer"/>，行为完全一致。
/// <see cref="NEnergyCounter"/> 通过 <see cref="NParticlesContainer"/> 类型引用调用 <c>Restart()</c> / <c>SetEmitting()</c>，
/// 因此必须继承原版类，而不是复制成独立的 <see cref="Godot.Node2D"/>。
/// </summary>
[GlobalClass]
public partial class NQueenParticlesContainer : NParticlesContainer;
