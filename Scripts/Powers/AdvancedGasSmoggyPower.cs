using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace ComicChess.TheQueen;

/// <summary>高级气体：回合结束时被烟雾侵蚀的牌变为爆炸。</summary>
public sealed class AdvancedGasSmoggyPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        _ = choiceContext;
        if (side != base.Owner.Side)
        {
            return;
        }

        IEnumerable<CardModel> allCards = base.Owner.Player?.PlayerCombatState?.AllCards ?? Array.Empty<CardModel>();
        List<CardModel> smogged = allCards.Where(static c => c.Affliction is Smog).ToList();
        if (smogged.Count == 0)
        {
            return;
        }

        Flash();
        foreach (CardModel card in smogged)
        {
            await CardCmd.TransformTo<Explode>(card, CardPreviewStyle.None);
        }
    }
}
