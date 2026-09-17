using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace ComicChess.TheQueen;

/// <summary>
/// 女王专用能量指示器。
/// 场景根节点挂本脚本；Ritsu 走 <see cref="NEnergyCounter"/> 工厂时会因 <c>source is NEnergyCounter</c> 而保留子类实例。
/// </summary>
[GlobalClass]
public partial class NQueenEnergyCounter : NEnergyCounter
{
}
