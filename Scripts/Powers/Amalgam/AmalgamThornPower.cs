using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

public sealed class AmalgamThornPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://images/atlases/power_atlas.sprites/thorns_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/thorns_power.png";

    private PlayerChoiceContext? _choiceContext;
    private Creature? _dealer;

    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return base.ModifyHpLostAfterOstyLate(target, amount, props, dealer, cardSource);
    }

    // 在女王受伤前，记录PlayerChoiceContext，用于后续反伤害
    public override Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        Creature? queen = base.Owner?.PetOwner?.Creature;
        if (queen == null)
            return Task.CompletedTask;

        if (target != queen)
            return Task.CompletedTask;

        _choiceContext = choiceContext;
        return Task.CompletedTask;
    }

    public override decimal ModifyHpLostBeforeOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        _dealer = target == base.Owner ? dealer : null;
        return amount;
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        if (_dealer == null || _choiceContext == null)
            return;

        // 反伤害
        Flash();
        await CreatureCmd.Damage(_choiceContext, _dealer, base.Amount, ValueProp.Unpowered | ValueProp.SkipHurtAnim, base.Owner, null);
        _dealer = null;
        _choiceContext = null;
    }
}