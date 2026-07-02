using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace ComicChess.TheQueen;

/// <summary>
/// 知识恶魔：你的回合开始时，从 <see cref="Rejuvenate"/> / <see cref="MindClarity"/> / <see cref="Disintegration"/> 中各选 1 张执行；
/// 层数即每回合选择次数。
/// </summary>
public sealed class KnowledgeDemonPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromCard<Rejuvenate>(),
		HoverTipFactory.FromCard<MindClarity>(),
		HoverTipFactory.FromCard<Disintegration>(),
	];

    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
		CombatState? combatState = base.Owner.CombatState;
		if (combatState == null || !base.Owner.IsAlive)
		{
			return;
		}

		int selections = (int)base.Amount;
		for (int i = 0; i < selections; i++)
		{
			await KnowledgeDemonCurseSelection.ChooseAndExecuteAsync(combatState, player);
			if (!base.Owner.IsAlive)
			{
				break;
			}
		}
	}
}
