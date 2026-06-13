using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace ComicChess.TheQueen;

/// <summary>提线木偶：倒计时结束时添加一份对应敌怪卡的卡牌奖励。</summary>
public sealed class MarionettePendingPower : QueenPowerModel
{
    private sealed class Data
    {
        public string MonsterId = string.Empty;
        public bool AllowUnknownSoulFallback = true;
    }

    protected override object? InitInternalData() => new Data();

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    internal void ConfigureMonsterId(string monsterId, bool allowUnknownSoulFallback)
    {
        Data data = GetInternalData<Data>();
        data.MonsterId = monsterId;
        data.AllowUnknownSoulFallback = allowUnknownSoulFallback;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _ = choiceContext;
        if (player != base.Owner.Player || !base.Owner.IsAlive)
        {
            return;
        }

        await PowerCmd.Decrement(this);
        if (Amount > 0m)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        if (!string.IsNullOrWhiteSpace(data.MonsterId))
        {
            CombatRoom? combatRoom = player.RunState?.CurrentRoom as CombatRoom;
            CardModel? rewardCard = MonsterCaptureRewardCatalog.CreateCaptureRewardCard(
                player,
                data.MonsterId,
                allowUnknownSoulFallback: data.AllowUnknownSoulFallback);
            if (combatRoom != null && rewardCard is not null)
            {
                combatRoom.AddExtraReward(player, new SpecialCardReward(rewardCard, player));
            }
        }

        await PowerCmd.Remove(this);
    }
}
