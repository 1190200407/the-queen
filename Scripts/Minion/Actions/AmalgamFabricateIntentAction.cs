using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：随机生成 1 张防御衍生牌�?1 张输出衍生牌到手牌�?/summary>
public sealed class AmalgamFabricateIntentAction : AmalgamActionModel
{
    public override string Key => "fabricate";

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_FABRICATE",
            _ => Task.CompletedTask,
            new AmalgamFabricateIntent());
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        if (amalgam.CombatState is not { } combatState
            || amalgam.PetOwner is not Player queen
            || !queen.Creature.IsAlive)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Cast", AmalgamGenerateCardIntentAction<Guard>.CastAnimDelay);

        await CreateOneDefensive(queen, combatState);
        await CreateOneOffensive(queen, combatState);
    }

    private static async Task CreateOneDefensive(Player owner, ICombatState combatState)
    {
        int roll = owner.RunState.Rng.CombatCardSelection.NextItem([0, 1]);
        switch (roll)
        {
            case 0:
                await QueenCardCmd.CreateInHand<Guard>(owner, combatState, false);
                break;
            default:
                await QueenCardCmd.CreateInHand<Noise>(owner, combatState, false);
                break;
        }
    }

    private static async Task CreateOneOffensive(Player owner, ICombatState combatState)
    {
        int roll = owner.RunState.Rng.CombatCardSelection.NextItem([0, 1]);
        switch (roll)
        {
            case 0:
                await QueenCardCmd.CreateInHand<Stab>(owner, combatState, false);
                break;
            default:
                await QueenCardCmd.CreateInHand<Zap>(owner, combatState, false);
                break;
        }
    }
}

