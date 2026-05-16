using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Cards.DynamicVars;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;


[RegisterCard(typeof(QueenCardPool))]
public sealed class GrantOffense : LearnIntentCardModel
{
    private const decimal learnIntentDamage = 4m;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(5m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentDamageVar(learnIntentDamage, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [QueenHoverTips.LearnIntent];
    protected override bool ShouldSummonBeforeLearnIntent => true;

    public GrantOffense()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal dmg = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateOffense(dmg);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Summon.UpgradeValueBy(2m);
    }
}
