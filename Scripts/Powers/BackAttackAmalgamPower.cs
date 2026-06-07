using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>后方攻击（朝左/聚合体）：该生物受到来自聚合体的伤害提高 50%。</summary>
public sealed class BackAttackAmalgamPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override bool ShouldPlayVfx => false;

    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/back_attack_right_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/back_attack_right_power.png";

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        _ = amount;
        _ = props;
        _ = cardSource;

        if (target != base.Owner || dealer is null)
        {
            return 1m;
        }

        return dealer.Monster is FriendlyAmalgam ? 1.5m : 1m;
    }
}

