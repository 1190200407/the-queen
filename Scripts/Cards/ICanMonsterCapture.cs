using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 单目标选敌时，是否对<strong>该目标</strong>展示捕获奖励卡意图预览。具体规则由实现类自行决定；
/// <paramref name="monster"/> 或 <paramref name="combatState"/> 为 null 时应返回 <c>false</c>。
/// </summary>
public interface ICanMonsterCapture
{
	bool CanCapture(MonsterModel monster, CombatState combatState);
}
