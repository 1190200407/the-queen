using System.Reflection;
using Godot.Bridge;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Patching.Core;

namespace ComicChess.TheQueen;

/// <summary>Mod 入口：ContentPack、图鉴筛选、类型发现与 Harmony 补丁。</summary>
[ModInitializer("Init")]
public class Entry
{
	public const string ModId = "sts2.comicchess.thequeen";
	public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

	// 魂灯免费出牌时，手牌描边高亮色。
	private static readonly Godot.Color SoulLampFreeGlow = new(196f / 255f, 242f / 255f, 3f / 255f, 0.98f);

	public static void Init()
	{
		var assembly = Assembly.GetExecutingAssembly();
		RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

		// ContentPack：卡牌关键词、手牌描边等运行时内容注册。
		// 关键词 id 由 localKeywordStem 派生（见 QueenKeyword）；文案在 localization/*/card_keywords.json。
		RitsuLibFramework.CreateContentPack(ModId)
			// 消逝：描述块显示在卡牌正文之前。
			.CardKeywordOwnedByLocNamespace(
				"Fade",
				"res://TheQueen/images/charui/queen_boss.png",
				ModKeywordCardDescriptionPlacement.BeforeCardDescription,
				includeInCardHoverTip: true)
			.CardKeywordOwnedByLocNamespace(
				nameof(QueenKeyword.AmalgamComposite),
				QueenKeyword.CompositeKeywordIconPath,
				ModKeywordCardDescriptionPlacement.None,
				includeInCardHoverTip: true)
			.Custom(static ctx => QueenKeyword.RegisterCompositeKeywords(ctx.Keywords))
			// 魂灯免费出牌时，为对应手牌加描边。
			.CardHandOutline<CardModel>(
				static card => SoulLampPower.IsCardFreeBySoulLamp(card) ? SoulLampFreeGlow : null,
				int.MinValue)
			.Apply();

		// 卡牌图鉴：怪物牌池筛选项（插在无色池之后）。
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

		// [Register*] attribute 扫描：角色、卡牌、遗物等。
		ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

		// Harmony 补丁；Apply 失败则整 mod 禁用。
		var patcher = RitsuLibFramework.CreatePatcher(ModId, "main", "the-queen");
		patcher.RegisterPatches<QueenModPatches>();
		RitsuLibFramework.ApplyRequiredPatcher(patcher, DisableMod);
	}

	private static void DisableMod()
	{
		Logger.Error("Required patches failed to apply; The Queen mod is disabled for this session.");
	}
}
