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
/// 胆小（聚合体版）：每回合第一次被命中（未格挡、攻击）时，为主人施加原版 <see cref="BlockNextTurnPower"/>；敌方回合结束后重置「本回合是否已触发」。
/// 与 <see cref="AmalgamCurlUpPower"/>（蜷身、每场战斗一次并移除自身）不同。
/// </summary>
public sealed class AmalgamSkittishPower : QueenPowerModel, IAmalgamEventListener
{
    private sealed class Data
    {
        public bool triggeredThisTurn;
    }

    protected override object? InitInternalData() => new Data();

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/skittish_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/skittish_power.png";

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromPower<BlockNextTurnPower>()];

    public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        _ = choiceContext;
        if (side != base.Owner.Side)
        {
            GetInternalData<Data>().triggeredThisTurn = false;
        }

        return Task.CompletedTask;
    }

    public async Task OnAmalgamHitAsync(
        CombatState combatState,
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
        await PowerCmd.Apply<BlockNextTurnPower>(new ThrowingPlayerChoiceContext(), queen, Amount, base.Owner, null);
    }
}
