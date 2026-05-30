using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 尖叫（聚合体改版）：生命值第一次降到阈值或以下时，改为沉睡 1 回合（施加 2 层 <see cref="AmalgamSleepPower"/>），并移除自身。
/// </summary>
public sealed class AmalgamShriekPower : QueenPowerModel
{
    private sealed class Data
    {
        public bool triggerd = false;
    }

    protected override object? InitInternalData() => new Data();

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    private const int _vulnerableStacks = 9;

    // 复用原版 SHRIEK_POWER 的图标资源。
    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/shriek_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/shriek_power.png";

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<VigorPower>()];

    private async Task CheckShriek(PlayerChoiceContext choiceContext)
    {
        if (base.Owner.CurrentHp > Amount)
        {
            return;
        }
        if (GetInternalData<Data>().triggerd)
        {
            return;
        }
        GetInternalData<Data>().triggerd = true;

        Flash();
        
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/terror_eel/terror_eel_debuff");
		await CreatureCmd.TriggerAnim(base.Owner, "Cast", 0f);
		await Cmd.Wait(0.3f);
		VfxCmd.PlayVfx(base.Owner.GetCreatureNode().VfxSpawnPosition, "vfx/vfx_scream", base.Owner.GetVfxContainer());
		await Cmd.CustomScaledWait(0.1f, 0.3f);
        foreach (var enemy in base.Owner.CombatState.Enemies)
        {
            if (enemy.IsAlive)
            {
                await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, _vulnerableStacks, base.Owner, null);
            }
        }
        await Cmd.Wait(0.5f);
        
        await PowerCmd.Apply<AmalgamSleepPower>(choiceContext, base.Owner, 1m, applier: base.Owner, cardSource: null);
        await PowerCmd.Remove(this);
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (Amount <= 0m)
        {
            return;
        }
        await CheckShriek(choiceContext);
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