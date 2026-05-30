using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Afflictions;

namespace ComicChess.TheQueen;

/// <summary>下回合开始时：抽 1，并为其附魔 <see cref="Dazed"/>，然后移除。</summary>
public sealed class NoisePendingPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Description
    {
        get
        {
            LocString d = new("powers", "NOISE_PENDING_POWER.description");
            d.Add("Draw", base.Amount);
            return d;
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != base.Owner || !base.Owner.IsAlive)
        {
            return;
        }

        int draw = System.Math.Max(0, (int)base.Amount);
        if (draw <= 0)
        {
            await PowerCmd.Remove(this);
            return;
        }

        IEnumerable<CardModel> drawn = await CardPileCmd.Draw(choiceContext, draw, player);
        foreach (CardModel card in drawn)
        {
            CardCmd.Enchant(ModelDb.Enchantment<Dazed>().ToMutable(), card, amount: 1m);
        }

        await PowerCmd.Remove(this);
    }
}

