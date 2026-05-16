
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Interop.AutoRegistration;

using STS2RitsuLib.Keywords;
namespace ComicChess.TheQueen;

[RegisterCharacterStarterCard(typeof(QueenCharacter), 1)]
[RegisterCard(typeof(QueenCardPool))]
public class SummonSoul : QueenCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<SoulStrike>(base.IsUpgraded), HoverTipFactory.FromCard<SoulDefend>(base.IsUpgraded),
        .. HoverTipFactory.FromAffliction<Bound>(), ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade), HoverTipFactory.FromPower<SoulLampPower>()];

    public SummonSoul()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.CombatState == null) return;

        await QueenCardCmd.CreateInHand<SoulStrike>(base.Owner, base.CombatState, base.IsUpgraded);
        await QueenCardCmd.CreateInHand<SoulDefend>(base.Owner, base.CombatState, base.IsUpgraded);

        await QueenCardCmd.AddSoulLamp(base.Owner);
    }
}