using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>吞噬：本回合玩家力量增益（回合结束失去）。</summary>
public sealed class DevourStrengthPower : TemporaryStrengthPower
{
	public override AbstractModel OriginModel => ModelDb.Card<Devour>();
}
