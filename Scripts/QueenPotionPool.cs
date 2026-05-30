using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace ComicChess.TheQueen;

public class QueenPotionPool : TypeListPotionPoolModel
{
    public override string EnergyColorName => "Queen";
    public override string? TextEnergyIconPath => "res://TheQueen/images/charui/text_energy.png";
    public override string? BigEnergyIconPath => "res://TheQueen/images/charui/big_energy.png";
    //rgb(155, 86, 205)
    public override Color LabOutlineColor => new(155f/255f, 86f/255f, 205f/255f);
}