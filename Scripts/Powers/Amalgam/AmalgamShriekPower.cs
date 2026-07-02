using System.Collections.Generic;
using System.Threading.Tasks;

using Godot;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
<<<<<<< HEAD
using MegaCrit.Sts2.Core.Nodes.Rooms;
=======
using MegaCrit.Sts2.Core.TestSupport;
>>>>>>> beta
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 尖叫（聚合体改版）：生命值第一次降到阈值或以下时，令所有敌人获得易伤、聚合体沉睡 1 回合，并移除自身。
/// 与原版 <see cref="ShriekPower"/> 相同，在 <see cref="AfterDamageReceived"/> 中判定；游戏逻辑须先于演出，避免联机下 <see cref="Creature.GetCreatureNode"/> 为空导致状态分歧。
/// </summary>
public sealed class AmalgamShriekPower : QueenPowerModel
{
	private sealed class Data
	{
		public bool Triggered;
	}

	protected override object? InitInternalData() => new Data();

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	private const int VulnerableStacks = 9;

	// 复用原版 SHRIEK_POWER 的图标资源。
	public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/shriek_power.tres";
	public override string? CustomBigIconPath => "res://images/powers/shriek_power.png";

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<VigorPower>()];

	public override async Task AfterDamageReceived(
		PlayerChoiceContext choiceContext,
		Creature target,
		DamageResult result,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource)
	{
		_ = props;
		_ = dealer;
		_ = cardSource;

		if (target != base.Owner || result.UnblockedDamage <= 0m || !base.Owner.IsAlive)
		{
			return;
		}

		await TryTriggerShriekAsync(choiceContext);
	}

	public override async Task AfterPowerAmountChanged(
		PlayerChoiceContext choiceContext,
		PowerModel power,
		decimal amount,
		Creature? applier,
		CardModel? cardSource)
	{
		_ = amount;
		_ = applier;
		_ = cardSource;

		if (power != this || Amount <= 0m)
		{
			return;
		}

		await TryTriggerShriekAsync(choiceContext);
	}

	private async Task TryTriggerShriekAsync(PlayerChoiceContext choiceContext)
	{
		if (base.Owner.CurrentHp > Amount || GetInternalData<Data>().Triggered)
		{
			return;
		}

		GetInternalData<Data>().Triggered = true;

		// 联机：所有端必须先一致地提交规则状态，再播 VFX（队友聚合体在本机可能没有 NCreature 节点）。
		if (base.Owner.CombatState is { } combatState)
		{
			foreach (Creature enemy in combatState.Enemies)
			{
				if (enemy.IsAlive)
				{
					await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, VulnerableStacks, base.Owner, null);
				}
			}
		}

		await PowerCmd.Apply<AmalgamSleepPower>(choiceContext, base.Owner, 1m, applier: base.Owner, cardSource: null);
		await PowerCmd.Remove(this);

		await PlayTriggerPresentationAsync();
	}

	private async Task PlayTriggerPresentationAsync()
	{
		if (TestMode.IsOn)
		{
			return;
		}

		Flash();
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/terror_eel/terror_eel_debuff");
		await CreatureCmd.TriggerAnim(base.Owner, "Cast", 0f);
		await Cmd.Wait(0.3f);
<<<<<<< HEAD
		if (NCombatRoom.Instance?.GetCreatureNode(base.Owner) is { } creatureNode)
		{
			VfxCmd.PlayVfx(creatureNode.VfxSpawnPosition, "vfx/vfx_scream");
		}
		await Cmd.CustomScaledWait(0.1f, 0.3f);
        foreach (var enemy in base.Owner.CombatState.Enemies)
        {
            if (enemy.IsAlive)
            {
                await PowerCmd.Apply<VulnerablePower>(enemy, _vulnerableStacks, base.Owner, null);
            }
        }
        await Cmd.Wait(0.5f);
        
        await PowerCmd.Apply<AmalgamSleepPower>(base.Owner, 1m, applier: base.Owner, cardSource: null);
        await PowerCmd.Remove(this);
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        _ = power;
        _ = amount;
        _ = applier;
        _ = cardSource;
        if (Amount <= 0m)
        {
            return;
        }
        await CheckShriek(new ThrowingPlayerChoiceContext());
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (target != base.Owner || !base.Owner.IsAlive)
        {
            return;
        }
        await CheckShriek(choiceContext);
    }
}
=======

		NCreature? node = base.Owner.GetCreatureNode();
		Control? vfxContainer = base.Owner.GetVfxContainer();
		if (node != null && vfxContainer != null)
		{
			VfxCmd.PlayVfx(node.VfxSpawnPosition, "vfx/vfx_scream", vfxContainer);
		}

		await Cmd.CustomScaledWait(0.1f, 0.3f);
		await Cmd.Wait(0.5f);
	}
}
>>>>>>> beta
