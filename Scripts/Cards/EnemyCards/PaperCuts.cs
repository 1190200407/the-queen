using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>纸伤难愈：拾起时，使之后战斗的所有敌人最大生命值 -2。本卡不加入牌组。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class PaperCuts : QueenCardModel
{
    private const int energyCost = -1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    private const int cutsOnPickup = 2;

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
    public override int MaxUpgradeLevel => 0;

    public PaperCuts()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override bool ShouldAddToDeck(CardModel card) => card is not PaperCuts;

    public override async Task AfterAddToDeckPrevented(CardModel card)
    {
        await PaperCutsRelic.AddCutsOnPickup(card.Owner, cutsOnPickup);
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Task.CompletedTask;
}

