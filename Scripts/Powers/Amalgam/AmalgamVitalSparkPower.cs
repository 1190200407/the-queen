using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 活力火花（聚合体版）：美术对齐原版 <c>VITAL_SPARK_POWER</c>；受击判定与聚合体「胆小」一致（<see cref="IAmalgamEventListener.OnAmalgamHitAsync"/> 的 <see cref="ValueProp.Move"/> / 未格挡等约定）。
/// 每回合第一次受到攻击伤害时，为主人施加原版 <see cref="EnergyNextTurnPower"/>（[blue]1[/blue] 点能量）。
/// </summary>
public sealed class AmalgamVitalSparkPower : QueenPowerModel, IAmalgamEventListener
{
    private sealed class Data
    {
        public bool triggeredThisTurn;
    }

    protected override object? InitInternalData() => new Data();

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/vital_spark_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/vital_spark_power.png";

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromPower<EnergyNextTurnPower>()];

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        _ = choiceContext;
        if (side != base.Owner.Side)
        {
            GetInternalData<Data>().triggeredThisTurn = false;
        }

        return Task.CompletedTask;
    }

    public async Task OnAmalgamHitAsync(
        ICombatState combatState,
        Creature amalgam,
        decimal unblockedDamage,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        _ = combatState;
        _ = dealer;
        _ = cardSource;

        if (amalgam != base.Owner || Amount <= 0m || unblockedDamage <= 0m)
        {
            return;
        }

        if (!props.HasFlag(ValueProp.Move) || props.HasFlag(ValueProp.Unpowered))
        {
            return;
        }

        Data data = GetInternalData<Data>();
        if (data.triggeredThisTurn)
        {
            return;
        }

        if (base.Owner.PetOwner?.Creature is not { IsAlive: true } queen)
        {
            return;
        }

        data.triggeredThisTurn = true;
        Flash();
        await PowerCmd.Apply<EnergyNextTurnPower>(new ThrowingPlayerChoiceContext(), queen, 1m, base.Owner, null);
    }
}
