using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>饥饿：可叠加。打出 <see cref="Devour"/> 时按层数获得魂灯（魂灯数量显示在吞噬卡面的 <c>HungerPower</c> 动态变量上）。</summary>
public sealed class HungerPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromCard<Devour>(upgrade: false),
		HoverTipFactory.FromPower<SoulLampPower>(),
	];
}
