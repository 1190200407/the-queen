using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>焕发：获得 1 点能量。</summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class Rejuvenate : QueenCardModel, KnowledgeDemon.IChoosable
{
    private const int energyCost = -1;
    private const CardType type = CardType.Status;
    private const CardRarity rarity = CardRarity.Status;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = false;

    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedInCombat => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    public Rejuvenate()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public Task OnChosen()
    {
        return PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
    }
}

