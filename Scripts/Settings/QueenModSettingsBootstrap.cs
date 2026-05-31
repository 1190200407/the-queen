using STS2RitsuLib;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils;
using STS2RitsuLib.Utils.Persistence;

namespace ComicChess.TheQueen;

/// <summary>注册女王 mod 设置页。</summary>
internal static class QueenModSettingsBootstrap
{
	private static readonly I18N ModSettingsI18N = RitsuLibFramework.CreateModLocalization(
		Entry.ModId,
		"mod_settings",
		pckFolders: ["res://TheQueen/localization/mod_settings"]);

	internal static void Register()
	{
		QueenSettingsStore.Register();

		RitsuLibFramework.RegisterModSettings(Entry.ModId, page => page
			.WithModDisplayName(T("THE_QUEEN_MOD.mod.displayName", "The Queen"))
			.WithTitle(T("THE_QUEEN_MOD.page.title", "The Queen"))
			.AddSection("visual", section => section
				.WithTitle(T("THE_QUEEN_MOD.section.visual.title", "Visual"))
				.AddToggle(
					"simplify_soul_lamp_vfx",
					T("THE_QUEEN_MOD.simplifySoulLampVfx.label", "Simplify soul lamp indicator VFX"),
					new ModSettingsValueBinding<QueenModSettings, bool>(
						Entry.ModId,
						"settings",
						SaveScope.Global,
						s => s.SimplifySoulLampIndicatorVfx,
						(s, value) => s.SimplifySoulLampIndicatorVfx = value),
					T("THE_QUEEN_MOD.simplifySoulLampVfx.description",
						"Hides soul lamp fire and particles on the energy indicator. The stack label remains visible."))));
	}

	private static ModSettingsText T(string key, string fallback)
	{
		return ModSettingsText.I18N(ModSettingsI18N, key, fallback);
	}
}
