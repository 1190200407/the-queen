using BaseLib.Abstracts;
using Godot;

namespace ComicChess.TheQueen;

public sealed class EnemyCardPool : CustomCardPoolModel
{
    /// <summary>
    /// 必须为 <c>true</c>：否则池子不会进入 <see cref="MegaCrit.Sts2.Core.Models.ModelDb.AllSharedCardPools"/>，
    /// <see cref="MegaCrit.Sts2.Core.Models.CardModel.Pool"/> 无法在 <see cref="MegaCrit.Sts2.Core.Models.ModelDb.AllCardPools"/> 中解析到本池，
    /// 预览/NCard 会误走 <c>MockCardPool</c> 并崩溃（见 BaseLib <see cref="BaseLib.Abstracts.CustomCardPoolModel"/> 注释）。
    /// </summary>
    public override bool IsShared => true;

    public override string Title => "Enemy";
    public override string? TextEnergyIconPath => "res://TheQueen/images/charui/text_energy.png";
    public override string? BigEnergyIconPath => "res://TheQueen/images/charui/big_energy.png";
    
    public override bool IsColorless => false;
    // 紫色rgb(69, 42, 112)
    public override Color DeckEntryCardColor => new(69f/255f, 42f/255f, 112f/255f);

    public override Color ShaderColor => new(69f/255f, 42f/255f, 112f/255f);
}