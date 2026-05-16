using System.Reflection;
using Godot.Bridge;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.Scaffolding.Cards.HandOutline;

namespace ComicChess.TheQueen;

[ModInitializer("Init")]
public class Entry
{
	public const string ModId = "sts2.comicchess.thequeen";
	public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

	private static readonly Godot.Color SoulLampFreeGlow = new(196f / 255f, 242f / 255f, 3f / 255f, 0.98f);

	public static void Init()
	{
		var assembly = Assembly.GetExecutingAssembly();
		RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

		RitsuLibFramework.CreateContentPack(ModId)
			.CardHandOutline<CardModel>(new ModCardHandOutlineRule(
				static card => SoulLampPower.IsCardFreeBySoulLamp(card),
				SoulLampFreeGlow,
				int.MinValue))
			.Apply();

		RitsuLibFramework.GetContentRegistry(ModId)
			.RegisterCardLibraryCompendiumSharedPoolFilter<EnemyCardPool>(
				"enemy",
				"res://TheQueen/images/charui/monster_card.png",
				[
					new CardLibraryCompendiumPlacementRule
					{
						VanillaFilterAnchorUniqueName = CardLibraryCompendiumVanillaFilterNames.ColorlessPool,
						Relation = CardLibraryCompendiumFilterInsertRelation.After,
					},
				]);

		ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

		var patcher = RitsuLibFramework.CreatePatcher(ModId, "main", "the-queen");
		patcher.RegisterPatches<QueenModPatches>();
		RitsuLibFramework.ApplyRequiredPatcher(patcher, DisableMod);
	}

	private static void DisableMod()
	{
		Logger.Error("Required patches failed to apply; The Queen mod is disabled for this session.");
	}
}
