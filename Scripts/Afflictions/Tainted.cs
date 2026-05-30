using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>污染（测试版 affliction，正式版已移除）。</summary>
[RegisterAffliction]
public sealed class Tainted : AfflictionModel
{
	public override bool IsStackable => true;

	public override bool HasExtraCardText => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		HoverTipFactory.FromPowerWithPowerHoverTips<TaintedPower>();

	public override bool CanAfflictCardType(CardType cardType) => cardType == CardType.Skill;
}
