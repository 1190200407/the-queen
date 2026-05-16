using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

internal sealed class PersonalHivePowerAmalgamDealerTransferPatch : IPatchMethod
{
	public static string PatchId => "thequeen_personal_hive_amalgam_dealer";
	public static string Description => "Personal Hive: redirect amalgam dealer to owner";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(PersonalHivePower), nameof(PersonalHivePower.AfterDamageReceived)),
	];

	public static bool Prefix(
		PersonalHivePower __instance,
		PlayerChoiceContext choiceContext,
		Creature target,
		[HarmonyArgument("_")] DamageResult damageResult,
		ValueProp props,
		ref Creature? dealer,
		CardModel? cardSource,
		ref Task __result)
	{
		_ = __instance;
		_ = choiceContext;
		_ = target;
		_ = damageResult;
		_ = props;
		_ = cardSource;

		if (dealer?.Monster is FriendlyAmalgam)
		{
			Creature? ownerCreature = dealer.PetOwner?.Creature;
			if (ownerCreature != null)
			{
				dealer = ownerCreature;
			}
		}

		if (dealer != null && dealer.Player is null)
		{
			__result = Task.CompletedTask;
			return false;
		}

		return true;
	}
}
