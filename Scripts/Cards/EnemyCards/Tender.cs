using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>柔嫩：力量、敏捷与原版 <see cref="TenderPower"/>；消耗。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class Tender : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    private const decimal statGain = 4m;
    private const decimal tenderStacks = 1m;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("StrengthGain", statGain),
        new IntVar("DexterityGain", statGain),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<TenderPower>(),
    ];

    public Tender()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        Creature self = base.Owner.Creature;
        await PowerCmd.Apply<StrengthPower>(self, base.DynamicVars["StrengthGain"].BaseValue, self, this);
        await PowerCmd.Apply<DexterityPower>(self, base.DynamicVars["DexterityGain"].BaseValue, self, this);
        await PowerCmd.Apply<TenderPower>(self, tenderStacks, self, this);
    }
}
