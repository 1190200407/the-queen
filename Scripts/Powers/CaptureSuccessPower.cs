using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace ComicChess.TheQueen;

/// <summary>
/// 通用「捕获成功」：战斗胜利结算时追加一张 <see cref="SpecialCardReward"/>；层数不叠加（<see cref="PowerStackType.Single"/>）。
/// 捕获牌在斩杀触发后调用 <see cref="ApplyForCapture"/> 并传入奖励用 <see cref="CardModel"/> 实例。
/// </summary>
public sealed class CaptureSuccessPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    /// <summary>战斗结束时发放的奖励牌（由捕获牌创建后赋值）。</summary>
    public CardModel? RewardCard { get; set; }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        RewardCard is null ? [] : [HoverTipFactory.FromCard(RewardCard)];

    /// <summary>由带「捕获」效果的卡牌在成功触发时调用。</summary>
    internal static async Task ApplyForCapture(Player owner, CardModel rewardCard, CardModel? captureSourceCard)
    {
        CaptureSuccessPower? applied =
            await PowerCmd.Apply<CaptureSuccessPower>(new ThrowingPlayerChoiceContext(), owner.Creature, 1m, owner.Creature, captureSourceCard);
        if (applied is not null)
        {
            applied.RewardCard = rewardCard;
        }
    }
}
