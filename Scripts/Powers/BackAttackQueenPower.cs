using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>后方攻击（朝右/女王）：该生物受到来自玩家的伤害提高 50%。</summary>
public sealed class BackAttackQueenPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override bool ShouldPlayVfx => false;

    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/back_attack_left_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/back_attack_left_power.png";

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        _ = amount;
        _ = props;
        _ = cardSource;

        if (target != base.Owner || dealer is null)
        {
            return 1m;
        }

        return dealer.IsPlayer ? 1.5m : 1m;
    }
}

