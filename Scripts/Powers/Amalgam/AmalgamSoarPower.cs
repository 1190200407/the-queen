using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>翱翔（聚合体版）：在落地之前受到的攻击伤害减少。</summary>
public sealed class AmalgamSoarPower : QueenPowerModel
{
    private const string DamageDecreaseKey = "DamageDecrease";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    // 复用原版 SoarPower 的图标资源。
    public override string? CustomPackedIconPath => "res://images/atlases/power_atlas.sprites/soar_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/soar_power.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(DamageDecreaseKey, 50m)
    ];


    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner || !base.Owner.IsAlive)
        {
            return amount;
        }

        return amount * base.DynamicVars[DamageDecreaseKey].BaseValue / 100m;
    }
    
    
    public override async Task AfterModifyingHpLostAfterOsty()
    {
        Flash();
        await Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await PowerCmd.Remove(this);
    }
}

