using Godot;
using MegaCrit.Sts2.addons.mega_text;

namespace ComicChess.TheQueen;

/// <summary>
/// <see cref="MegaLabel"/> 的 mod 子类：默认关闭自动字号，使用场景里设置的 <c>font_size</c>。
/// </summary>
[GlobalClass]
public partial class NewMegaLabel : MegaLabel
{
}
