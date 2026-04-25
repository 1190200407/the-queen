using System.Linq;
using System.Reflection;
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

/// <summary>聚合体意图：按目标模式对敌人施加原版晕眩能力。</summary>
public sealed class AmalgamApplyStunIntentAction : AmalgamActionModel
{
    private static readonly string[] StunPowerTypeCandidates =
    [
        "MegaCrit.Sts2.Core.Models.Powers.StunPower, sts2",
        "MegaCrit.Sts2.Core.Models.Powers.StunnedPower, sts2",
        "MegaCrit.Sts2.Core.Models.Powers.StunMonsterPower, sts2"
    ];

    public AmalgamApplyStunIntentAction(decimal turns)
        : base(turns)
    {
    }

    public static readonly float CastAnimDelay = 1.5f;

    public override LocString IntentTitle => new("monsters", "FRIENDLY_AMALGAM.intent_stun.title");

    public override LocString GetIntentDescription()
    {
        return new LocString("monsters", "FRIENDLY_AMALGAM.intent_stun.description");
    }

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_STUN",
            _ => Task.CompletedTask,
            new AmalgamApplyStunIntent());
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        _ = choiceContext;
        CombatState? combatState = amalgam.CombatState;
        if (combatState == null || amalgam.PetOwner is not Player queen || !queen.Creature.IsAlive)
        {
            return;
        }

        Creature[] alive = combatState.Enemies.Where(e => e.IsAlive).ToArray();
        if (alive.Length == 0 || Amount <= 0m)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Cast", CastAnimDelay);
        AmalgamOffenseTargetingMode mode = AmalgamOffenseTargeting.ResolveMode(combatState, queen);
        Creature applier = queen.Creature;

        if (mode == AmalgamOffenseTargetingMode.AllAliveEnemies)
        {
            foreach (Creature enemy in alive)
            {
                await ApplyStunPower(enemy, applier);
            }

            return;
        }

        if (mode == AmalgamOffenseTargetingMode.LockedMarkedEnemy)
        {
            Creature? marked = AmalgamOffenseTargeting.FindMarkedEnemy(combatState);
            if (marked is { IsAlive: true })
            {
                await ApplyStunPower(marked, applier);
            }

            return;
        }

        Creature randomEnemy = alive[System.Random.Shared.Next(alive.Length)];
        await ApplyStunPower(randomEnemy, applier);
    }

    private async Task ApplyStunPower(Creature target, Creature applier)
    {
        Type? stunPowerType = ResolveStunPowerType();
        if (stunPowerType == null)
        {
            return;
        }

        MethodInfo? applyGeneric = typeof(PowerCmd)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => m.Name == nameof(PowerCmd.Apply)
                && m.IsGenericMethodDefinition
                && m.GetGenericArguments().Length == 1
                && m.GetParameters().Length >= 4);
        if (applyGeneric == null)
        {
            return;
        }

        MethodInfo apply = applyGeneric.MakeGenericMethod(stunPowerType);
        object? taskObj = apply.Invoke(null, [target, Amount, applier, null, false]);
        if (taskObj is Task task)
        {
            await task;
        }
    }

    private static Type? ResolveStunPowerType()
    {
        foreach (string candidate in StunPowerTypeCandidates)
        {
            if (Type.GetType(candidate) is { } type)
            {
                return type;
            }
        }

        return null;
    }
}
