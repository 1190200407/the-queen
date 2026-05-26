using Godot;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace ComicChess.TheQueen;

[RegisterSharedCardPool]
public sealed class EnemyCardPool : TypeListCardPoolModel
{
    public override string Title => "Enemy";
    public override string EnergyColorName => "Enemy";
    public override string? TextEnergyIconPath => "res://TheQueen/images/charui/text_energy.png";
    public override string? BigEnergyIconPath => "res://TheQueen/images/charui/big_energy.png";
    
    public override bool IsColorless => false;
    // 紫色rgb(55, 68, 148)
    public override Color DeckEntryCardColor => new(55f/255f, 68f/255f, 148f/255f);

    private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateRgbShaderMaterial(69f/255f, 42f/255f, 112f/255f);
    public override Material? PoolFrameMaterial => _poolFrameMaterial;
}