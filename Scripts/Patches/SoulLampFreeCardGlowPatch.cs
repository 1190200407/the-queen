using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class SoulLampFreeCardGlowPatch : IPatchMethod
{
	private static readonly Color SoulLampFreeGlow = new(196f / 255f, 242f / 255f, 3f / 255f, 0.98f);

	public static string PatchId => "thequeen_soul_lamp_free_card_glow";
	public static string Description => "Yellow-green hand glow for soul-lamp free bound cards";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NHandCardHolder), nameof(NHandCardHolder.UpdateCard)),
	];

	public static void Postfix(NHandCardHolder __instance)
	{
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
