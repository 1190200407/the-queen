using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>
/// 聚合体意图：对随机敌人造成 Amount 点伤害。
/// </summary>
public sealed class AmalgamOffenseIntentAction : AmalgamActionModel
{
    public AmalgamOffenseIntentAction(decimal damage) : base(damage)
    {
    }

    public override LocString IntentTitle => new("monsters", "FRIENDLY_AMALGAM.intent_offense.title");

    public override LocString GetIntentDescription()
    {
        var desc = new LocString("monsters", "FRIENDLY_AMALGAM.intent_offense.description");
        desc.Add("Amount", Amount);
        return desc;
    }

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_OFFENSE",
            _ => Task.CompletedTask,
            new AmalgamSingleAttackIntent(Amount));
    }

    /// <remarks>意图条 <c>PerformIntent</c> 仅在回合末执行灯槽记录时由 <see cref="FriendlyAmalgam.BeforeTurnEnd"/> 调用；此处只打出伤害链。</remarks>
    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        var combatState = amalgam.CombatState;
        if (combatState == null)
        {
            return;
        }

        Creature[] enemies = combatState.Enemies.Where(e => e.IsAlive).ToArray();
        if (enemies.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Shared.Next(enemies.Length);
        Creature randomEnemy = enemies[randomIndex];

        await FriendlyAmalgamCmd.ExecuteSingleTargetAttack(
            choiceContext,
            amalgam,
            randomEnemy,
            Amount,
            "Attack",
            0.6f,
            "vfx/vfx_attack_blunt");
    }
}

