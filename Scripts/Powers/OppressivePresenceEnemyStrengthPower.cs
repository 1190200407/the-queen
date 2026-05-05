using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>压迫感：使目标本回合失去力量（回合结束恢复）。</summary>
public sealed class OppressivePresenceEnemyStrengthPower : TemporaryStrengthPower
{
	public override AbstractModel OriginModel => ModelDb.Card<OppressivePresence>();

	protected override bool IsPositive => false;
}
