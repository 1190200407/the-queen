using BaseLib.Abstracts;
using Godot;

namespace ComicChess.TheQueen;

public class QueenPotionPool : CustomPotionPoolModel
{
    public override string? TextEnergyIconPath => "res://TheQueen/images/charui/text_energy.png";
    public override string? BigEnergyIconPath => "res://TheQueen/images/charui/big_energy.png";
}