using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 原版 <see cref="NCombatRoom.AddCreature"/> 仅对 <c>Osty</c> 使用女王侧偏移；友方聚合体走随从脚边一行。
/// 在每次随从加入场景后，将本地主控视角下的友方聚合体拉回奥斯提同款槽位（含其它随从加入时被脚边布局带偏的情况）。
/// </summary>
internal sealed class NCombatRoomAddCreatureFriendlyAmalgamPatch : IPatchMethod
{
	public static string PatchId => "thequeen_ncombatroom_add_creature_amalgam_layout";
	public static string Description => "Place friendly amalgam beside queen when AddCreature runs";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NCombatRoom), nameof(NCombatRoom.AddCreature)),
	];

	public static void Postfix(Creature creature)
	{
		if (creature.PetOwner is not Player owner)
		{
			return;
		}

		ICombatState? combatState = owner.Creature.CombatState;
		if (combatState == null)
		{
			return;
		}

		Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, owner);
		if (amalgam == null)
		{
			return;
		}

		FriendlyAmalgamCmd.ApplyLocalAmalgamSlot(owner, amalgam, combatState);
	}
}
