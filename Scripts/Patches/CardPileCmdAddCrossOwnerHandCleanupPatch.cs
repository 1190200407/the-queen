using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;

namespace ComicChess.TheQueen;

/// <summary>
/// 原版 <see cref="CardPileCmd.Add(System.Collections.Generic.IEnumerable{CardModel},CardPile,CardPilePosition,AbstractModel?,bool)"/> 在把牌加入「非本地玩家」的手牌时，
/// 依赖 <see cref="MegaCrit.Sts2.Core.Nodes.Cards.NCard.FindOnTable"/> 做动画；但 <c>card.Pile == null</c> 时 FindOnTable 恒为 null，
/// 无法清理仍挂在本地 <see cref="NPlayerHand"/>（含选牌区）上的旧节点。典型场景：递交牌先 <see cref="CardModel.RemoveFromCurrentPile"/> 再改 <see cref="CardModel.Owner"/> 后加入女王手牌。
/// 清理时用原版能力牌同款 <see cref="NCardFlyPowerVfx"/>（此时 <see cref="CardModel.Owner"/> 已是接收方，轨迹飞向该角色）。
/// </summary>
[HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Add), new[]
{
	typeof(IEnumerable<CardModel>),
	typeof(CardPile),
	typeof(CardPilePosition),
	typeof(AbstractModel),
	typeof(bool),
})]
internal static class CardPileCmdAddCrossOwnerHandCleanupPatch
{
	[HarmonyPriority(200)]
	[HarmonyPrefix]
	private static void Prefix(IEnumerable<CardModel> cards, CardPile newPile)
	{
		if (TestMode.IsOn || !LocalContext.NetId.HasValue || !CombatManager.Instance.IsInProgress)
		{
			return;
		}

		if (newPile is not { Type: PileType.Hand })
		{
			return;
		}

		NCombatRoom? combatRoom = NCombatRoom.Instance;
		NPlayerHand? hand = combatRoom?.Ui.Hand;
		Control? vfxRoot = combatRoom?.CombatVfxContainer;
		if (hand == null)
		{
			return;
		}

		foreach (CardModel card in cards)
		{
			if (card?.Owner == null || LocalContext.IsMe(card.Owner))
			{
				continue;
			}

			if (hand.GetCardHolder(card) == null)
			{
				continue;
			}

			NCard? nCard = hand.GetCard(card);
			if (nCard == null)
			{
				hand.Remove(card);
				continue;
			}

			Vector2 flyFrom = nCard.GlobalPosition;
			hand.Remove(card);
			if (vfxRoot == null)
			{
				nCard.QueueFreeSafely();
				continue;
			}

			vfxRoot.AddChildSafely(nCard);
			nCard.GlobalPosition = flyFrom;

			NCardFlyPowerVfx? powerFly = NCardFlyPowerVfx.Create(nCard);
			if (powerFly == null)
			{
				nCard.QueueFreeSafely();
				continue;
			}

			vfxRoot.AddChildSafely(powerFly);
			_ = TaskHelper.RunSafely(powerFly.PlayAnim());
		}
	}
}
