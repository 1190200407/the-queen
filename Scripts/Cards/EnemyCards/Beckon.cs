using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>呼唤：召唤并学习「施加呼唤」意图。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class Beckon : LearnIntentCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 怪物牌不可升级：10(14) 落地为 14。
        new SummonVar(15m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new IntVar("LearnIntentBeckon", 12m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<BeckonPower>(),
    ];

    protected override bool ShouldSummonBeforeLearnIntent => true;
    public override int MaxUpgradeLevel => 0;

    public Beckon()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal stacks = base.DynamicVars["LearnIntentBeckon"].BaseValue;
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>(
        [
            new AmalgamApplyDebuffIntentAction<BeckonPower>(stacks, debuffEntryId: "COMICCHESS-BECKON_POWER"),
        ]);
    }
}

