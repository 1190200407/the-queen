using System.Linq;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

internal static class QueenHoverTips
{
	private static readonly string CaptureHoverTipId = new HoverTip(
		new LocString("static_hover_tips", "capture.title"),
		new LocString("static_hover_tips", "capture.description")).Id;

	/// <summary>卡面合并后的 <see cref="CardModel.HoverTips"/> 是否包含「捕获」说明（与 <see cref="Capture"/> 同源 Id）。</summary>
	internal static bool CardHasCaptureHoverTip(CardModel card) =>
		card.HoverTips.Any(static t => t.Id == CaptureHoverTipId);

	internal static IHoverTip SoulLamp => new HoverTip(
		new LocString("static_hover_tips", "soul_lamp.title"),
		new LocString("static_hover_tips", "soul_lamp.description")
	);

	internal static IHoverTip BindingOath => new HoverTip(
		new LocString("static_hover_tips", "binding_oath.title"),
		new LocString("static_hover_tips", "binding_oath.description")
	);

	internal static IHoverTip LearnIntent => new HoverTip(
		new LocString("static_hover_tips", "learn_intent.title"),
		new LocString("static_hover_tips", "learn_intent.description")
	);

	internal static IHoverTip ForgetIntent => new HoverTip(
		new LocString("static_hover_tips", "forget_intent.title"),
		new LocString("static_hover_tips", "forget_intent.description")
	);

	internal static IHoverTip Capture => new HoverTip(
		new LocString("static_hover_tips", "capture.title"),
		new LocString("static_hover_tips", "capture.description"));
}
