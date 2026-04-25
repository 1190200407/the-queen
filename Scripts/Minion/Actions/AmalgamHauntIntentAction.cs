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

/// <summary>hauntAction：三个意图合成一个 action（虚弱 + 易伤 + 本回合失去力量）。</summary>
public sealed class AmalgamHauntIntentAction : AmalgamActionModel
{
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

    public override LocString IntentTitle => new("intents", "AMALGAM_HAUNT.title");

    public override LocString GetIntentDescription()
    {
        // 灯槽悬停以 MoveState.Intents 为准；这里提供无战斗上下文时的兜底文案。
        LocString desc = new("intents", "AMALGAM_HAUNT.description");
        desc.Add("Weak", _weak);
        desc.Add("Vulnerable", _vulnerable);
        desc.Add("StrengthLoss", _strengthLoss);
        return desc;
    }

    protected override MoveState CreateMoveState()
    {
        // 注意：action 合并，但 intent 仍保留 3 个（用于 UI 类型/图标/文案）。
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
        CombatState? combatState = amalgam.CombatState;
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
            await PowerCmd.Apply<WeakPower>(target, _weak, applier, null);
            await PowerCmd.Apply<VulnerablePower>(target, _vulnerable, applier, null);
            await PowerCmd.Apply<AmalgamIntentStrengthDownPower>(target, _strengthLoss, applier, null);
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

        Creature randomEnemy = alive[System.Random.Shared.Next(alive.Length)];
        await ApplyAll(randomEnemy);
    }
}

