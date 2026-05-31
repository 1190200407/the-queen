namespace ComicChess.TheQueen;

/// <summary>女王 mod 全局设置（持久化于 <c>settings.json</c>）。</summary>
public sealed class QueenModSettings
{
	/// <summary>开启后隐藏魂灯指示器的火焰与粒子，仅保留层数标签与图层逻辑。</summary>
	public bool SimplifySoulLampIndicatorVfx { get; set; }
}
