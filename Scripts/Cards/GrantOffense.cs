using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class GrantOffense : QueenCardModel
{
    private const decimal learnIntentDamage = 4m;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(5m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentDamageVar(learnIntentDamage, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [QueenHoverTips.LearnIntent];

    public GrantOffense()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);
        // 意图内只存原始基础伤害；力量等在 CreatureCmd.Damage 中由 Hook 叠一次。
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateOffense(learnIntentDamage);
        await FriendlyAmalgamCmd.LearnIntent(choiceContext, base.Owner, intent, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Summon.UpgradeValueBy(2m);
    }
}
