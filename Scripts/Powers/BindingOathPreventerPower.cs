using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;

namespace ComicChess.TheQueen;

/// <summary>
/// 仅作为 ShouldPlay 的 <c>preventer</c>，使气泡显示「魂缚誓约」而非当前卡牌名。
/// 不要对生物执行 PowerCmd.Apply。
/// </summary>
public sealed class BindingOathPreventerPower : QueenPowerModel
{
	public override PowerType Type => PowerType.None;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override LocString Title => new LocString("static_hover_tips", "binding_oath.title");
}
