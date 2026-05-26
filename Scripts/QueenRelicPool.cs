using Godot;
using STS2RitsuLib.Scaffolding.Content;
namespace ComicChess.TheQueen;

public class QueenRelicPool : TypeListRelicPoolModel
{
    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://TheQueen/images/charui/text_energy.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://TheQueen/images/charui/big_energy.png";
    public override string EnergyColorName => "Queen";
    //rgb(155, 86, 205)
    public override Color LabOutlineColor => new(155f/255f, 86f/255f, 205f/255f);
}