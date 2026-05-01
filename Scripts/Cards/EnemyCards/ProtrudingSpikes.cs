using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>突出尖刺：召唤并学习获得荆棘。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class ProtrudingSpikes : LearnIntentCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(10m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new IntVar("LearnIntentThorns", 5m),
    ];

    public override int MaxUpgradeLevel => 0;

    protected override bool ShouldSummonBeforeLearnIntent => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        HoverTipFactory.FromPower<AmalgamThornPower>(),
    ];

    public ProtrudingSpikes()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal thorns = base.DynamicVars["LearnIntentThorns"].BaseValue;
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>(
        [
            AmalgamActionRegistry.CreateGainBuff<AmalgamThornPower>(thorns, buffEntryId: "COMICCHESS-AMALGAM_THORN_POWER"),
        ]);
    }
}
