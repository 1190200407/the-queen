using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 沙坑（女王版）：计数归零时使所有敌人直接死亡；回合结束时计数 -1。类名不可为 <c>SandpitPower</c>（与原版 ModelId 冲突）。
/// </summary>
public sealed class AmalgamSandpitPower : QueenPowerModel
{
    /// <summary>仅当计数经 <see cref="PowerCmd.ModifyAmount"/> 减至 0 并移除时为 true；死亡清能力时不应触发斩杀。</summary>
    private bool _triggerExecuteWhenRemoved;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 复用原版 Sandpit 的图标资源。
    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/sandpit_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/sandpit_power.png";

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        _ = choiceContext;
        _ = participants;
        if (side != base.Owner.Side || Amount <= 0m)
        {
            return;
        }

        await PowerCmd.Decrement(this);
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        _ = choiceContext;
        _ = amount;
        _ = applier;
        _ = cardSource;
        if (power == this && Amount <= 0m)
        {
            _triggerExecuteWhenRemoved = true;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        if (oldOwner.IsDead || !_triggerExecuteWhenRemoved)
        {
            return;
        }
        if (base.Owner?.CombatState is not { } combatState)
        {
            return;
        }

		IReadOnlyList<Creature> allAffectedCreature = combatState.Enemies.Where(static c => c.IsAlive).ToArray();
		foreach (Creature enemy in allAffectedCreature)
		{
			await CreatureCmd.Kill(enemy, force: true);
		}

		if (base.Owner.Player is { } player)
		{
			await CreatureCmd.TriggerAnim(base.Owner, "Attack", player.Character.AttackAnimDelay);
		}
	}
}
