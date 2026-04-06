using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;

namespace ComicChess.TheQueen;

/// <summary>
/// 魂缚锁链：本回合由锁链抽牌逻辑施加魂缚的牌（每回合最多 <see cref="PowerModel.Amount"/> 张），
/// 回合结束时只清除这些牌上的魂缚，不动牌面自带的魂缚。
/// </summary>
internal static class ChainsOfBindingBoundTracker
{
	private sealed class State
	{
		internal int AppliedThisTurn;
		internal readonly HashSet<CardModel> ChainsCards = new();
	}

	private static readonly Dictionary<ulong, State> _byPlayerNetId = new();

	internal static void ResetCombat()
	{
		_byPlayerNetId.Clear();
	}

	private static State GetOrCreate(ulong netId)
	{
		if (!_byPlayerNetId.TryGetValue(netId, out State? s))
		{
			s = new State();
			_byPlayerNetId[netId] = s;
		}

		return s;
	}

	/// <summary>本回合是否还能再对一张牌施加锁链魂缚（张数上限 = <paramref name="maxCardsPerTurn"/>）。</summary>
	internal static bool CanApplyAnotherCard(Player? player, int maxCardsPerTurn)
	{
		if (player == null || maxCardsPerTurn <= 0)
		{
			return false;
		}

		return GetOrCreate(player.NetId).AppliedThisTurn < maxCardsPerTurn;
	}

	/// <summary> 注册抽到的牌 </summary>
	internal static void RegisterCardDrawn(Player? player)
	{
		if (player == null)
		{
			return;
		}

		GetOrCreate(player.NetId).AppliedThisTurn++;
	}

	/// <summary>在已成功调用 <see cref="CardCmd.AfflictAndPreview{T}"/> 后登记该牌。</summary>
	internal static void RegisterChainsBoundCard(Player? player, CardModel card)
	{
		if (player == null)
		{
			return;
		}

		State s = GetOrCreate(player.NetId);
		s.ChainsCards.Add(card);
	}

	/// <summary>回合结束：仅移除锁链本回合加在「非消耗堆」牌上的魂缚，并清空计数。</summary>
	internal static void ClearChainsBoundsEndOfTurn(Player? player)
	{
		if (player == null || !_byPlayerNetId.TryGetValue(player.NetId, out State? s))
		{
			return;
		}

		foreach (CardModel c in s.ChainsCards.ToList())
		{
			if (c.Affliction is Bound)
			{
				CardCmd.ClearAffliction(c);
			}
		}

		s.ChainsCards.Clear();
		s.AppliedThisTurn = 0;
	}
}
