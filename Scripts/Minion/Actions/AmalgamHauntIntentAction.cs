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

/// <summary>hauntAction????????? action??? + ?? + ?????????</summary>
public sealed class AmalgamHauntIntentAction : AmalgamActionModel
{
    public override string Key => "haunt";

    private readonly decimal _weak;
    private readonly decimal _vulnerable;
    private readonly decimal _strengthLoss;
    private readonly Creature? _forcedTarget;

    public AmalgamHauntIntentAction(decimal weak, decimal vulnerable, decimal strengthLoss)
        : base(0m)
    {
        _weak = weak;
        _vulnerable = vulnerable;
        _strengthLoss = strengthLoss;
    }

    public AmalgamHauntIntentAction(decimal weak, decimal vulnerable, decimal strengthLoss, Creature? forcedTarget)
        : this(weak, vulnerable, strengthLoss)
    {
        _forcedTarget = forcedTarget;
    }

    public static readonly float CastAnimDelay = 1.5f;

    protected override MoveState CreateMoveState()
    {
        // ???action ???? intent ??? 3 ???? UI ??/??/????
        return new MoveState(
            "AMALGAM_INTENT_HAUNT",
            _ => Task.CompletedTask,
            new AmalgamApplyWeakIntent(_weak),
            new AmalgamApplyVulnerableIntent(_vulnerable),
            new AmalgamStrengthDownIntent(_strengthLoss));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        _ = choiceContext;
        ICombatState? combatState = amalgam.CombatState;
        if (combatState == null || amalgam.PetOwner is not Player queen || !queen.Creature.IsAlive)
        {
            return;
        }

        Creature[] alive = combatState.Enemies.Where(e => e.IsAlive).ToArray();
        if (alive.Length == 0 || _weak <= 0m || _vulnerable <= 0m || _strengthLoss <= 0m)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Cast", CastAnimDelay);

        AmalgamOffenseTargetingMode mode = AmalgamOffenseTargeting.ResolveMode(combatState, queen);
        Creature applier = queen.Creature;

        async Task ApplyAll(Creature target)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, target, _weak, applier, null);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, target, _vulnerable, applier, null);
            await PowerCmd.Apply<AmalgamIntentStrengthDownPower>(choiceContext, target, _strengthLoss, applier, null);
        }

        if (_forcedTarget is { IsAlive: true } forcedTarget && alive.Contains(forcedTarget))
        {
            await ApplyAll(forcedTarget);
            return;
        }

        if (mode == AmalgamOffenseTargetingMode.AllAliveEnemies)
        {
            foreach (Creature enemy in alive)
            {
                await ApplyAll(enemy);
            }

            return;
        }

        if (mode == AmalgamOffenseTargetingMode.LockedMarkedEnemy)
        {
            Creature? marked = AmalgamOffenseTargeting.FindMarkedEnemy(combatState);
            if (marked is { IsAlive: true })
            {
                await ApplyAll(marked);
            }

            return;
        }

        Creature? randomEnemy = FriendlyAmalgamCmd.NextRandomHittableEnemy(queen, alive);
        if (randomEnemy is not { IsAlive: true })
        {
            return;
        }

        await ApplyAll(randomEnemy);
    }
}
