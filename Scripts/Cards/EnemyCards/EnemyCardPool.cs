using Godot;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ComicChess.TheQueen;

[RegisterSharedCardPool]
public sealed class EnemyCardPool : TypeListCardPoolModel
{
    public override string Title => "Enemy";
    public override string EnergyColorName => "Enemy";
    public override string? TextEnergyIconPath => "res://TheQueen/images/charui/text_energy.png";
    public override string? BigEnergyIconPath => "res://TheQueen/images/charui/big_energy.png";
    
    public override bool IsColorless => false;
    // 紫色rgb(72, 79, 177)
    public override Color DeckEntryCardColor => new(72f/255f, 79f/255f, 177f/255f);
    // 能量图标轮廓颜色rgb(44, 97, 24)
    public override Color EnergyOutlineColor => new(44f/255f, 97f/255f, 24f/255f);

    private static readonly Material? _poolFrameMaterial = QueenPoolFrameMaterials.FromRgb(72, 79, 177);
    public override Material? PoolFrameMaterial => _poolFrameMaterial;
}