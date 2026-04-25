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

/// <summary>聚合体意图：对聚合体以外所有单位施加若干回合的缩小（原版 <see cref="ShrinkPower"/>）。</summary>
public sealed class AmalgamShrinkRayIntentAction : AmalgamActionModel
{
    public AmalgamShrinkRayIntentAction(decimal turns)
        : base(turns)
    {
    }

    public static readonly float CastAnimDelay = 1.5f;

    public override LocString IntentTitle => new("intents", "AMALGAM_SHRINK_RAY.title");

    public override LocString GetIntentDescription()
    {
        LocString desc = new("intents", "AMALGAM_SHRINK_RAY.description");
        desc.Add("Stacks", Amount);
        return desc;
    }

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

        List<Creature> targets = new();
        foreach (Player p in combatState.Players)
        {
            if (p.Creature is { IsAlive: true } pc && !ReferenceEquals(pc, amalgam))
            {
                targets.Add(pc);
            }
        }

        foreach (Creature enemy in combatState.Enemies.Where(e => e.IsAlive))
        {
            if (!ReferenceEquals(enemy, amalgam))
            {
                targets.Add(enemy);
            }
        }

        if (targets.Count == 0)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Cast", CastAnimDelay);
        await PowerCmd.Apply<ShrinkPower>(targets, Amount, queen.Creature, null);
    }
}

