using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
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
    public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == null)
            return;

        if (base.Owner == null || !base.Owner.IsAlive)
            return;

        Creature? queen = base.Owner.PetOwner?.Creature;
        if (queen == null)
            return;

        if (target != queen)
            return;

        if (props.HasFlag(ValueProp.Unpowered) || !props.HasFlag(ValueProp.Move))
            return;

        FriendlyAmalgam? amalgam = base.Owner.Monster as FriendlyAmalgam;
        if (amalgam == null || amalgam.IsSleeping())
            return;
        
        await CreatureCmd.Damage(choiceContext, dealer, base.Amount, ValueProp.Unpowered | ValueProp.SkipHurtAnim, base.Owner, null);
    }
}