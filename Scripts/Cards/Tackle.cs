using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(TokenCardPool))]
public sealed class Tackle : LearnIntentCardModel
{
    private const decimal learnIntentDamage = 5m;
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new AmalgamLearnIntentDamageVar(learnIntentDamage, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ModKeywordRegistry.GetCardKeyword(QueenKeyword.Fade)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [QueenHoverTips.LearnIntent];

    public Tackle()
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
        base.DynamicVars["LearnIntentDamage"].UpgradeValueBy(2m);
    }
}
