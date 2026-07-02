using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>狂乱牵引：沙坑计数 -1；本卡耗能 +1。</summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class FranticTug : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Status;
    private const CardRarity rarity = CardRarity.Status;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    public override int MaxUpgradeLevel => 0;

    public FranticTug()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        if (base.Owner?.Creature is not { } self)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(self, "Cast", base.Owner.Character.CastAnimDelay);

        if (self.GetPower<AmalgamSandpitPower>() is { } sandpit)
        {
            await PowerCmd.ModifyAmount(choiceContext, sandpit, -1m, self, this);
        }

        // 提高这张牌自己的基础耗能（下次再抽到会更贵）。
        base.EnergyCost.UpgradeBy(1);
    }
}

