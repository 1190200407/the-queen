using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace ComicChess.TheQueen;

public class QueenPotionPool : TypeListPotionPoolModel
{
    public override string EnergyColorName => "Queen";
    public override string? TextEnergyIconPath => "res://TheQueen/images/charui/text_energy.png";
    public override string? BigEnergyIconPath => "res://TheQueen/images/charui/big_energy.png";
    // #814390
    public override Color LabOutlineColor => new(161f/255f, 67f/255f, 144f/255f);
}