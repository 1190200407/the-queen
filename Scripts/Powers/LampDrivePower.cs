using System.Collections.Generic;
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

namespace ComicChess.TheQueen;

<<<<<<< HEAD
/// <summary>灯驱：本回合每消耗 1 点魂灯，聚合体行动 1 次；回合结束时移除。</summary>
=======
/// <summary>灯驱（可叠加）：本回合每消耗 1 点魂灯，聚合体按层数执行行动；回合结束时移除。</summary>
>>>>>>> beta
public sealed class LampDrivePower : QueenPowerModel, ISoulLampEventListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<SoulLampPower>(),
    ];

    public async Task OnSoulLampAmountChanged(
        PlayerChoiceContext choiceContext,
        Player player,
        decimal delta,
        Creature? applier,
        CardModel? cardSource)
    {
        _ = applier;
        _ = cardSource;
<<<<<<< HEAD
        if (delta >= 0m || player != base.Owner.Player)
=======
        if (delta >= 0m || player != base.Owner?.Player)
>>>>>>> beta
        {
            return;
        }

<<<<<<< HEAD
        if (base.CombatState is not CombatState combatState)
=======
        if (base.CombatState is not ICombatState combatState)
>>>>>>> beta
        {
            return;
        }

        Creature? amalgamCreature = FriendlyAmalgamCmd.GetExisting(combatState, player);
        if (amalgamCreature?.Monster is not FriendlyAmalgam amalgam || !amalgamCreature.IsAlive)
        {
            return;
        }

        int totalActions = (int)(-delta);
        if (totalActions <= 0)
        {
            return;
        }
<<<<<<< HEAD

=======
        
        int totalActions = (int)-delta * actionsPerSoulLamp;
>>>>>>> beta
        Flash();
        for (int i = 0; i < totalActions; i++)
        {
            if (!amalgamCreature.IsAlive || CombatManager.Instance.IsOverOrEnding)
            {
                break;
            }

            await amalgam.ActCurrentIntentImmediatelyAsync(choiceContext);

            if (CombatManager.Instance.IsOverOrEnding)
            {
                break;
            }
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
