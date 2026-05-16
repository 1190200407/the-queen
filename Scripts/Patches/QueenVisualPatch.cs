using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class BigMushroomGrowScalePatch : IPatchMethod
{
	public static string PatchId => "thequeen_big_mushroom_grow_scale";
	public static string Description => "Big Mushroom grow uses 1.2x scale for queen";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(BigMushroom), "Grow"),
	];

	public static bool Prefix(BigMushroom __instance)
	{
		NCombatRoom.Instance?.GetCreatureNode(__instance.Owner.Creature)?.ScaleTo(1.2f, 0f);
		return false;
	}
}

internal sealed class SurroundedPowerQueenFacingPatch : IPatchMethod
{
	public static string PatchId => "thequeen_surrounded_flip_scale";
	public static string Description => "Invert Surrounded flip logic for queen facing";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(SurroundedPower), "FlipScale", new[] { typeof(Node2D) }),
	];

	public static bool Prefix(SurroundedPower __instance, Node2D? body, ref Task __result)
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
			body.Scale = new Vector2(-body.Scale.X, body.Scale.Y);
		}

		__result = Task.CompletedTask;
		return false;
	}
}
