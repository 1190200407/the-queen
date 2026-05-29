using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>呼唤之力：每回合将原版呼唤加入手牌；回合结束时若手牌无呼唤，你获得无实体。</summary>
public sealed class BeckonPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<MegaCrit.Sts2.Core.Models.Cards.Beckon>(),
        HoverTipFactory.FromPower<IntangiblePower>(),
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != base.Owner.Player || base.CombatState is not ICombatState combatState)
        {
            return;
        }

        CardModel beckon = combatState.CreateCard<MegaCrit.Sts2.Core.Models.Cards.Beckon>(player);
        Flash();
        await CardPileCmd.AddGeneratedCardToCombat(beckon, PileType.Hand, player);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        _ = choiceContext;
        _ = participants;
        if (side != base.Owner.Side || !base.Owner.IsAlive)
        {
            return;
        }

        Player? player = base.Owner.Player;
        if (player?.PlayerCombatState?.Hand.Cards.Any(static c => c is MegaCrit.Sts2.Core.Models.Cards.Beckon) == true)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<IntangiblePower>(new ThrowingPlayerChoiceContext(), base.Owner, 1m, base.Owner, null);
    }
}
