using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>许愿 ver2：生成三张衍生能力牌并获得 1 点魂灯。</summary>
[RegisterCard(typeof(QueenCardPool))]
public sealed class WishVer2 : QueenCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<WishFameAndFortune>(upgrade: base.IsUpgraded),
        HoverTipFactory.FromCard<WishLongevity>(upgrade: base.IsUpgraded),
        HoverTipFactory.FromCard<WishMountainStrength>(upgrade: base.IsUpgraded),
        HoverTipFactory.FromPower<SoulLampPower>(),
        .. HoverTipFactory.FromAffliction<Bound>(),
    ];

    public WishVer2()
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

        await QueenCardCmd.CreateInHand<WishFameAndFortune>(base.Owner, base.CombatState, base.IsUpgraded);
        await QueenCardCmd.CreateInHand<WishLongevity>(base.Owner, base.CombatState, base.IsUpgraded);
        await QueenCardCmd.CreateInHand<WishMountainStrength>(base.Owner, base.CombatState, base.IsUpgraded);
        await QueenCardCmd.AddSoulLamp(choiceContext, base.Owner, 1);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
    }
}
