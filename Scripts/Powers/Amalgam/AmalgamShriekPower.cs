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

    // 复用原版 SHRIEK_POWER 的图标资源。
    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/shriek_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/shriek_power.png";

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<VigorPower>()];

    private async Task CheckShriek()
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
        // 施加 2 层：怪物回合命中后很快进入玩家回合开始，AmalgamSleepPower 会立刻 -1。
        await PowerCmd.Apply<AmalgamSleepPower>(base.Owner, 1m, applier: base.Owner, cardSource: null);
        await PowerCmd.Apply<VigorPower>(base.Owner, 7m, base.Owner, null);
        await PowerCmd.Remove(this);
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (Amount <= 0m)
        {
            return;
        }
        await CheckShriek();
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (target != base.Owner || !base.Owner.IsAlive)
        {
            return;
        }
        await CheckShriek();
    }
}