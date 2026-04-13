using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public class SummonSoul : QueenCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SoulStrike>(base.IsUpgraded), HoverTipFactory.FromCard<SoulDefend>(base.IsUpgraded),
        .. HoverTipFactory.FromAffliction<Bound>(), HoverTipFactory.FromKeyword(QueenKeyword.fade), QueenHoverTips.SoulLamp];

    public SummonSoul()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.CombatState == null) return;

        // 生成两张牌后，给它们附加原版 Bound(魂缚)。
        // 这样 Bound 的“每回合限制打出次数/回合结束解除”等规则才能正确生效。
        await QueenCardCmd.CreateInHand<SoulStrike>(base.Owner, base.CombatState, base.IsUpgraded);
        await QueenCardCmd.CreateInHand<SoulDefend>(base.Owner, base.CombatState, base.IsUpgraded);

        await QueenCardCmd.AddSoulLamp(base.Owner);
    }
}