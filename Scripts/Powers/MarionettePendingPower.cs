using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>提线木偶：倒计时结束时将对应敌怪卡永久加入牌组。</summary>
public sealed class MarionettePendingPower : QueenPowerModel
{
    private sealed class Data
    {
        public string MonsterId = string.Empty;
    }

    protected override object? InitInternalData() => new Data();

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override bool IsInstanced => true;

    internal void ConfigureMonsterId(string monsterId)
    {
        Data data = GetInternalData<Data>();
        data.MonsterId = monsterId;
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
            CardModel? cardToAdd = MonsterCaptureRewardCatalog.TryCreateCaptureRewardCard(player, data.MonsterId);
            if (cardToAdd is not null)
            {
                List<AbstractModel> sources = [this];
                CardModel modified = Hook.ModifyCardBeingAddedToDeck(player.RunState!, cardToAdd, out sources);
                AbstractModel? source = this;
                if (Hook.ShouldAddToDeck(player.RunState!, modified, out source))
                {
                    player.Deck.AddInternal(modified, player.Deck.Cards.Count, silent: false);
                }
            }
        }

        await PowerCmd.Remove(this);
    }
}
