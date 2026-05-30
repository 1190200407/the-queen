using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

/// <summary>念动力：生成续念、断念。消耗（升级后 0 费）。</summary>
[RegisterCard(typeof(QueenCardPool))]
public sealed class Psychokinesis : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<ContinueIntent>(upgrade: false),
        HoverTipFactory.FromCard<RenounceIntent>(upgrade: false),
        ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade),
    ];

    public Psychokinesis()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (base.CombatState == null)
        {
            return;
        }

        await QueenCardCmd.CreateInHand<ContinueIntent>(base.Owner, base.CombatState);
        await QueenCardCmd.CreateInHand<RenounceIntent>(base.Owner, base.CombatState);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
