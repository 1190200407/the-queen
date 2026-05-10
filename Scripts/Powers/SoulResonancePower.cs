using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 灵魂同调：打出触发 <see cref="FriendlyAmalgamCmd.LearnIntent"/> / <see cref="FriendlyAmalgamCmd.CombineIntent"/> 的牌后，为其他存活玩家镜像同一意图；
/// 学习路径要求牌带 <see cref="QueenCardTags.LearnIntent"/>；合并路径对其他聚合体使用同一 <c>compositeIndexKey</c>。
/// </summary>
/// <remarks>
/// <see cref="PlayerChoiceContext"/> 标识<strong>当前这一次</strong>出牌/指令解析过程（会沿着 Cmd 调用链向下传递）；
/// 向他人镜像时仍传入<strong>同一个</strong> context，故用一张 <see cref="ConditionalWeakTable{TKey, TValue}"/> 标记「本 context 下已由外层扩散过」，嵌套的 <c>AfterLearnIntent</c> / <c>AfterCombineIntent</c> 不再扩散，避免多名玩家都具备本能力时的往复触发。
/// </remarks>
public sealed class SoulResonancePower : QueenPowerModel, IAmalgamEventListener
{
	private static readonly ConditionalWeakTable<PlayerChoiceContext, object> SpreadLock = new();

	private static readonly object SpreadSentinel = new();

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public async Task AfterLearnIntent(
		CombatState combatState,
		PlayerChoiceContext choiceContext,
		Player amalgamOwner,
		Creature amalgam,
		AmalgamActionModel intent,
		AbstractModel? source)
	{
		if (base.Owner.Player != amalgamOwner)
		{
			return;
		}

		if (source is not CardModel playedCard || !playedCard.Tags.Contains(QueenCardTags.LearnIntent))
		{
			return;
		}

		if (playedCard.Owner != amalgamOwner)
		{
			return;
		}

		if (SpreadLock.TryGetValue(choiceContext, out _))
		{
			return;
		}

		SpreadLock.Add(choiceContext, SpreadSentinel);
		try
		{
			foreach (Player other in combatState.Players)
			{
				if (other.NetId == amalgamOwner.NetId || !other.Creature.IsAlive)
				{
					continue;
				}

				await FriendlyAmalgamCmd.LearnIntent(choiceContext, other, intent.Clone(), source);
			}
		}
		finally
		{
			SpreadLock.Remove(choiceContext);
		}
	}

	public async Task AfterCombineIntent(
		CombatState combatState,
		PlayerChoiceContext choiceContext,
		Player amalgamOwner,
		Creature amalgam,
		AmalgamActionModel intent,
		AbstractModel? source,
		string? compositeIndexKey)
	{
		if (base.Owner.Player != amalgamOwner)
		{
			return;
		}

		if (source is not CardModel playedCard)
		{
			return;
		}

		if (playedCard.Owner != amalgamOwner)
		{
			return;
		}

		if (SpreadLock.TryGetValue(choiceContext, out _))
		{
			return;
		}

		SpreadLock.Add(choiceContext, SpreadSentinel);
		try
		{
			foreach (Player other in combatState.Players)
			{
				if (other.NetId == amalgamOwner.NetId || !other.Creature.IsAlive)
				{
					continue;
				}

				await FriendlyAmalgamCmd.CombineIntent(choiceContext, other, intent.Clone(), source, compositeIndexKey);
			}
		}
		finally
		{
			SpreadLock.Remove(choiceContext);
		}
	}
}
