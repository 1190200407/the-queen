using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：对所有敌方单位施加若干回合的缩小（原�?<see cref="ShrinkPower"/>）�?/summary>
public sealed class AmalgamShrinkRayIntentAction : AmalgamSingleDecimalActionModel
{
    public override string Key => "shrink_ray";

    public AmalgamShrinkRayIntentAction()
    {
    }

    public AmalgamShrinkRayIntentAction(decimal turns)
        : base(turns)
    {
    }

    public override bool Init(decimal amount) => TryInitSingleDecimal(amount);

    public override bool Init(object[] args) => TryInitSingleDecimal(args);

    public static readonly float CastAnimDelay = 1.5f;

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_SHRINK_RAY",
            _ => Task.CompletedTask,
            new AmalgamShrinkRayIntent(Amount));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        CombatState? combatState = amalgam.CombatState;
        if (combatState == null || amalgam.PetOwner is not Player queen || !queen.Creature.IsAlive)
        {
            return;
        }

        if (Amount == 0m)
        {
            return;
        }

        List<Creature> targets = combatState.Enemies.Where(e => e.IsAlive).ToList();

        if (targets.Count == 0)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Cast", CastAnimDelay);
        await PowerCmd.Apply<ShrinkPower>(targets, Amount, queen.Creature, null);
    }
}

