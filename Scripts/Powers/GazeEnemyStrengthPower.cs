using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>注视：目标本回合失去力量（回合结束恢复）。</summary>
public sealed class GazeEnemyStrengthPower : TemporaryStrengthPower
{
	public override AbstractModel OriginModel => ModelDb.Card<Gaze>();

	protected override bool IsPositive => false;
}
