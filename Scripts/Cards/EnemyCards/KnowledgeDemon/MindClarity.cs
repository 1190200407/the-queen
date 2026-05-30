using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Monsters;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>心灵澄明：抽 1 张牌。</summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class MindClarity : QueenCardModel, KnowledgeDemon.IChoosable
{
    private const int energyCost = -1;
    private const CardType type = CardType.Status;
    private const CardRarity rarity = CardRarity.Status;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = false;

    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedInCombat => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    public MindClarity()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public async Task OnChosen()
    {
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), base.DynamicVars.Cards.IntValue, base.Owner);
    }
}

