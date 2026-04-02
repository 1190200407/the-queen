using BaseLib.Abstracts;
using Godot;

namespace ComicChess.TheQueen;

public class QueenRelicPool : CustomRelicPoolModel
{
    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://TheQueen/images/charui/text_energy.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://TheQueen/images/charui/big_energy.png";
}