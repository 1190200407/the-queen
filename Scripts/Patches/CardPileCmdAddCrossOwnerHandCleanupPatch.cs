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
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class CardPileCmdAddCrossOwnerHandCleanupPatch : IPatchMethod
{
	public static string PatchId => "thequeen_card_pile_cmd_cross_owner_hand_cleanup";
	public static string Description => "Clean local hand UI when cards move to another player's hand";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CardPileCmd), nameof(CardPileCmd.Add), new[]
		{
			typeof(IEnumerable<CardModel>),
			typeof(CardPile),
			typeof(CardPilePosition),
			typeof(AbstractModel),
			typeof(bool),
			typeof(bool),
		}),
	];

	[HarmonyPriority(200)]
	public static void Prefix(IEnumerable<CardModel> cards, CardPile newPile)
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
