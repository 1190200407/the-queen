using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>……无趣：按弃牌张数获得的本回合力量（回合结束失去）。</summary>
public sealed class JoylessStrengthPower : TemporaryStrengthPower
{
	public override AbstractModel OriginModel => ModelDb.Card<Joyless>();
}
