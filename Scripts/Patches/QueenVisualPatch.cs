using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ComicChess.TheQueen;

/// <summary>
/// BigMushroom 默认 Grow 为 1.5 倍，女王模型会偏大并越界；改为 1.2 倍。
/// </summary>
[HarmonyPatch(typeof(BigMushroom), "Grow")]
internal static class BigMushroomGrowScalePatch
{
	[HarmonyPrefix]
	private static bool Prefix(BigMushroom __instance)
	{
		NCombatRoom.Instance?.GetCreatureNode(__instance.Owner.Creature)?.ScaleTo(1.2f, 0f);
		return false;
	}
}

/// <summary>
/// 「被包围」会按左右朝向翻转玩家模型；女王底图默认朝向与原版相反，
/// 因此当该效果试图翻面时，需要把翻转条件取反。
/// </summary>
[HarmonyPatch(typeof(SurroundedPower))]
internal static class SurroundedPowerQueenFacingPatch
{
	[HarmonyPrefix]
	[HarmonyPatch("FlipScale")]
	private static bool FlipScalePrefix(SurroundedPower __instance, Node2D? body, ref Task __result)
	{
		if (__instance.Owner?.Player?.Character is not QueenCharacter)
		{
			return true;
		}

		if (body == null)
		{
			__result = Task.CompletedTask;
			return false;
		}

		float x = body.Scale.X;
		SurroundedPower.Direction facing = __instance.Facing;
		bool shouldFlip = (facing == SurroundedPower.Direction.Right && x > 0f)
			|| (facing == SurroundedPower.Direction.Left && x < 0f);
		if (shouldFlip)
		{
			// Only invert facing sign; preserve current animation scale magnitude.
			body.Scale = new Vector2(-body.Scale.X, body.Scale.Y);
		}

		__result = Task.CompletedTask;
		return false;
	}
}
