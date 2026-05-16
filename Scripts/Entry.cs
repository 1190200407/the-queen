using System.Reflection;
using Godot.Bridge;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;

namespace ComicChess.TheQueen;

[ModInitializer("Init")]
public class Entry
{
	public const string ModId = "sts2.comicchess.thequeen";
	public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

	public static void Init()
	{
		var assembly = Assembly.GetExecutingAssembly();
		RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

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
