using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>填装射击：召唤并学习“格挡 + 下回合伤害”意图；自带魂缚。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class ReloadFire : LearnIntentCardModel
{
    // 怪物牌不可升级：召唤3(5) 落地为 5；格挡3(4) 落地为 4；下回合伤害14(16) 落地为 16。
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(5m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentBlockVar(4m),
        new AmalgamLearnIntentDamageVar(16m, ValueProp.Move),
    ];

    public override int MaxUpgradeLevel => 0;
    protected override bool ShouldSummonBeforeLearnIntent => true;
    internal override bool HasSelfBound => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..HoverTipFactory.FromAffliction<Bound>(),
    ];

    public ReloadFire()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal block = base.DynamicVars["LearnIntentBlock"].BaseValue;
        decimal dmg = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>(
        [
            new AmalgamReloadFireIntentAction(block, dmg),
        ]);
    }
}

