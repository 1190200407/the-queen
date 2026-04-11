using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;

namespace ComicChess.TheQueen;

/// <summary>
/// 因魂灯免费打出的魂缚牌：手牌高亮改为黄绿色（RGB 196,242,3）。
/// 优先级在红/金高亮之后：若 ShouldGlowRed/ShouldGlowGold 命中则不覆盖。
/// </summary>
[HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder.UpdateCard))]
internal static class SoulLampFreeCardGlowPatch
{
	private static readonly Color SoulLampFreeGlow = new(196f / 255f, 242f / 255f, 3f / 255f, 0.98f);

	[HarmonyPostfix]
	private static void Postfix(NHandCardHolder __instance)
	{
		// UpdateCard 在 !IsNodeReady() 时会提前 return，但 Harmony Postfix 仍会执行；此时 NCard 可能未 _Ready，CardHighlight 为空。
		if (!GodotObject.IsInstanceValid(__instance) || !__instance.IsNodeReady())
		{
			return;
		}

		CombatManager? combat = CombatManager.Instance;
		if (combat == null || !combat.IsInProgress || !combat.IsPlayPhase)
		{
			return;
		}

		NCard? cardNode = __instance.CardNode;
		if (!GodotObject.IsInstanceValid(cardNode) || !cardNode.IsNodeReady() || cardNode.Model == null)
		{
			return;
		}

		NCardHighlight highlight = cardNode.CardHighlight;
		if (!GodotObject.IsInstanceValid(highlight))
		{
			return;
		}

		CardModel model = cardNode.Model;
		if (model.ShouldGlowRed || model.ShouldGlowGold)
		{
			return;
		}
		if (!model.CanPlay())
		{
			return;
		}
		if (!SoulLampPower.IsCardFreeBySoulLamp(model))
		{
			return;
		}

		highlight.Modulate = SoulLampFreeGlow;
	}
}
