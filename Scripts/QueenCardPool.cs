using BaseLib.Abstracts;
using Godot;

namespace ComicChess.TheQueen;

public class QueenCardPool : CustomCardPoolModel
{
    public override string Title => "Queen";
    public override string? TextEnergyIconPath => "res://TheQueen/images/charui/text_energy.png";
    public override string? BigEnergyIconPath => "res://TheQueen/images/charui/big_energy.png";
    public override bool IsColorless => false;
    // 紫色rgb(112, 42, 112)
    public override Color DeckEntryCardColor => new(112f/255f, 42f/255f, 112f/255f);
}