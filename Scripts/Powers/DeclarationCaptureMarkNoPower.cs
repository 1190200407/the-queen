using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>宣告标记（不可捕获）：与可捕获版相同文案与回合计数，不触发捕获；图标为 <c>declaration_capture_mark_no</c>。</summary>
public sealed class DeclarationCaptureMarkNoPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new IntVar("Upgraded", 0),
	];

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		if (cardSource is Declaration declaration)
		{
			base.DynamicVars["Upgraded"].BaseValue = declaration.IsUpgraded ? 1 : 0;
		}

		return Task.CompletedTask;
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		_ = choiceContext;
		_ = player;
		await PowerCmd.Decrement(this);
		if (Amount <= 0m)
		{
			await PowerCmd.Remove(this);
		}
	}
}
