using STS2RitsuLib;
using STS2RitsuLib.Data;
using STS2RitsuLib.Utils.Persistence;

namespace ComicChess.TheQueen;

/// <summary>读取女王 mod 持久化设置。</summary>
public static class QueenSettingsStore
{
	private const string SettingsKey = "settings";
	private const string SettingsFileName = "settings.json";

	private static readonly ModDataStore Store = ModDataStore.For(Entry.ModId);
	private static readonly object InitLock = new();
	private static bool _initialized;

	internal static void Register()
	{
		lock (InitLock)
		{
			if (_initialized)
			{
				return;
			}

			using (RitsuLibFramework.BeginModDataRegistration(Entry.ModId))
			{
				Store.Register(
					SettingsKey,
					SettingsFileName,
					SaveScope.Global,
					() => new QueenModSettings(),
					autoCreateIfMissing: true);
			}

			_initialized = true;
		}
	}

	/// <summary>是否简化魂灯指示器特效（隐藏火焰与粒子）。</summary>
	public static bool IsSoulLampIndicatorVfxSimplified()
	{
		EnsureInitialized();
		return Store.Get<QueenModSettings>(SettingsKey).SimplifySoulLampIndicatorVfx;
	}

	private static void EnsureInitialized()
	{
		if (!_initialized)
		{
			Register();
		}
	}
}
