using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>信息素喷射：召唤并学习「人体蜂房 + 力量」意图；魂缚。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class PheromoneSpit : LearnIntentCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(10m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new PowerVar<AmalgamPersonalHivePower>(1m),
        new IntVar("LearnIntentStrength", 1m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<AmalgamPersonalHivePower>(),
        ..HoverTipFactory.FromEnchantment<Dazed>(),
    ];

    public override int MaxUpgradeLevel => 0;

    protected override bool ShouldSummonBeforeLearnIntent => true;

    internal override bool HasSelfBound => true;

    public PheromoneSpit()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal hive = base.DynamicVars.Power<AmalgamPersonalHivePower>().BaseValue;
        decimal strength = base.DynamicVars["LearnIntentStrength"].BaseValue;
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>(
        [
            new AmalgamGainBuffAndStrengthIntentAction<AmalgamPersonalHivePower>(
                hive,
                buffEntryId: "COMICCHESS-AMALGAM_PERSONAL_HIVE_POWER",
                strength),
        ]);
    }
}

