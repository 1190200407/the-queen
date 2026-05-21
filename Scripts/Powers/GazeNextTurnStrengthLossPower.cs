using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Players;
namespace ComicChess.TheQueen;

/// <summary>
/// 注视：跳过下一次「敌方回合开始」触发（本回合已由 <see cref="GazeEnemyStrengthPower"/> 处理），
/// 再下一次敌方回合开始时按层数施加等量本回合力量损失，然后移除。
/// </summary>
public sealed class GazeNextTurnStrengthLossPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
		if (!base.Owner.IsAlive)
		{
			await PowerCmd.Remove(this);
			return;
		}

		if (base.Amount <= 0m)
		{
			return;
		}

		Creature? applier = base.Applier;
		if (applier == null)
		{
			await PowerCmd.Remove(this);
			return;
		}

		await PowerCmd.Apply<GazeEnemyStrengthPower>(choiceContext, base.Owner, base.Amount, applier, null);
		await PowerCmd.Remove(this);
	}
}
