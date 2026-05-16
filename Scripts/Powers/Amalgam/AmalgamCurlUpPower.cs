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
/// 蜷身（聚合体版）：对齐原版 <c>CURL_UP_POWER</c> 美术；被命中时对主人施加原版 <see cref="BlockNextTurnPower"/> 并移除此能力（每场战斗一次，由移除保证）。
/// </summary>
public sealed class AmalgamCurlUpPower : QueenPowerModel, IAmalgamEventListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/curl_up_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/curl_up_power.png";

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

        if (base.Owner.PetOwner?.Creature is not { IsAlive: true } queen)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<BlockNextTurnPower>(new ThrowingPlayerChoiceContext(), queen, Amount, base.Owner, null);
        await PowerCmd.Remove(this);
    }
}
