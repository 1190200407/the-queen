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

/// <summary>易伤孢子：召唤；学习意图为通用 <see cref="AmalgamApplyVulnerableIntentAction"/>（施加易伤）。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class FrailSpores : QueenCardModel
{
    private const decimal learnIntentVulnerable = 2m;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(3m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentVulnerableVar(learnIntentVulnerable),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        QueenHoverTips.LearnIntent,
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    public FrailSpores()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);
        decimal stacks = base.DynamicVars["LearnIntentVulnerable"].BaseValue;
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateVulnerable(stacks);
        await FriendlyAmalgamCmd.LearnIntent(choiceContext, base.Owner, intent, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Summon.UpgradeValueBy(2m);
        base.DynamicVars["LearnIntentVulnerable"].UpgradeValueBy(1m);
    }
}
