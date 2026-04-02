using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace ComicChess.TheQueen;

internal static class QueenHoverTips
{
	internal static IHoverTip SoulLamp => new HoverTip(
		new LocString("static_hover_tips", "soul_lamp.title"),
		new LocString("static_hover_tips", "soul_lamp.description")
	);

	internal static IHoverTip BindingOath => new HoverTip(
		new LocString("static_hover_tips", "binding_oath.title"),
		new LocString("static_hover_tips", "binding_oath.description")
	);
}
