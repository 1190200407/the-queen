using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class LuoYeGuiGen : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<GuiYuZhiYe>(upgrade: base.IsUpgraded),
        HoverTipFactory.FromCard<GuiYuChenTu>(upgrade: base.IsUpgraded),
        HoverTipFactory.FromPower<SoulLampPower>()
    ];

    public LuoYeGuiGen()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (base.CombatState == null)
        {
            return;
        }

        await QueenCardCmd.CreateInHand<GuiYuZhiYe>(base.Owner, base.CombatState, isUpgraded: base.IsUpgraded);
        await QueenCardCmd.CreateInHand<GuiYuChenTu>(base.Owner, base.CombatState, isUpgraded: base.IsUpgraded);
        await QueenCardCmd.AddSoulLamp(base.Owner, 1);
    }
}
