using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>长刺：召唤并学习「获得荆棘」意图。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class Spiken : LearnIntentCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    private const decimal thorns = 3m;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 怪物牌不可升级：把 5(7) 落地为 7。
        new SummonVar(7m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
    ];

    public override int MaxUpgradeLevel => 0;
    protected override bool ShouldSummonBeforeLearnIntent => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<AmalgamThornPower>(),
    ];

    public Spiken()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>(
        [
            AmalgamActionRegistry.CreateGainBuff<AmalgamThornPower>(thorns, buffEntryId: "AMALGAM_THORN_POWER"),
        ]);
    }
}

