using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 硬化外壳（聚合体版）：本回合内，聚合体最多失去 <see cref="PowerModel.Amount"/> 点生命值；每回合开始重置。
/// （行为参考原版 Hardened Shell。）
/// </summary>
public sealed class AmalgamHardenedShellPower : QueenPowerModel
{
    private sealed class Data
    {
        public decimal Remaining;
        public bool InitializedThisTurn;
    }

    protected override object? InitInternalData() => new Data();

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => (int)Math.Max(0, GetInternalData<Data>().Remaining);

    // 复用原版 Hardened Shell 的图标资源。
    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/hardened_shell_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/hardened_shell_power.png";

    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        _ = props;
        _ = dealer;
        _ = cardSource;

        if (target != base.Owner || amount <= 0m || Amount <= 0m)
        {
            return amount;
        }

        Data data = GetInternalData<Data>();
        if (!data.InitializedThisTurn)
        {
            data.Remaining = Amount;
            data.InitializedThisTurn = true;
        }

        if (data.Remaining <= 0m)
        {
            InvokeDisplayAmountChanged();
            return 0m;
        }

        decimal allowed = amount > data.Remaining ? data.Remaining : amount;
        data.Remaining -= allowed;
        InvokeDisplayAmountChanged();
        return allowed;
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        Flash();
        await Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _ = choiceContext;
        if (base.Owner.PetOwner is not Player queen || player != queen)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        data.Remaining = Amount;
        data.InitializedThisTurn = true;
        InvokeDisplayAmountChanged();
        await Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        _ = amount;
        _ = applier;
        _ = cardSource;

        // 在“获得/叠加”时立刻刷新一次显示（让 DisplayAmount 立即生效）。
        if (power == this)
        {
            Data data = GetInternalData<Data>();
            data.Remaining = Amount;
            data.InitializedThisTurn = true;
            InvokeDisplayAmountChanged();
        }

        return Task.CompletedTask;
    }
}

