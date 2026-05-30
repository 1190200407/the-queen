using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>灯驱（可叠加）：本回合每获得 1 点魂灯，聚合体按层数执行行动；回合结束时移除。</summary>
public sealed class LampDrivePower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<SoulLampPower>(),
    ];

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (amount <= 0m || power is not SoulLampPower || power.Owner != base.Owner)
        {
            return;
        }

        if (base.CombatState is not CombatState combatState)
        {
            return;
        }

        Player? player = base.Owner.Player;
        if (player == null)
        {
            return;
        }

        Creature? amalgamCreature = FriendlyAmalgamCmd.GetExisting(combatState, player);
        if (amalgamCreature?.Monster is not FriendlyAmalgam amalgam || !amalgamCreature.IsAlive)
        {
            return;
        }

        int actionsPerSoulLamp = (int)Amount;
        if (actionsPerSoulLamp <= 0)
        {
            return;
        }

        int totalActions = (int)amount * actionsPerSoulLamp;
        Flash();
        for (int i = 0; i < totalActions; i++)
        {
            if (!amalgamCreature.IsAlive)
            {
                break;
            }

            await amalgam.ActCurrentIntentImmediatelyAsync(choiceContext);
        }
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        _ = choiceContext;
        if (side == base.Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}
