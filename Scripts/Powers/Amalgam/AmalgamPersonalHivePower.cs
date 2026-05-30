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
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 人体蜂房（聚合体版）：美术对齐原版 <c>PERSONAL_HIVE_POWER</c>；受到伤害时由主人抽牌并为抽到的牌附魔 <see cref="Dazed"/>（与原版往抽牌堆加入晕眩牌不同）。
/// </summary>
public sealed class AmalgamPersonalHivePower : QueenPowerModel, IAmalgamEventListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/personal_hive_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/personal_hive_power.png";
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [..HoverTipFactory.FromEnchantment<Dazed>()];

    public async Task OnAmalgamHitAsync(ICombatState combatState, Creature amalgam, decimal unblockedDamage, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (amalgam != base.Owner || Amount <= 0m)
        {
            return;
        }

        if (base.Owner.PetOwner is not { } player || player.Creature is not { IsAlive: true })
        {
            return;
        }

        Flash();
        EnchantmentModel dazedTemplate = ModelDb.Enchantment<Dazed>().ToMutable();
        IEnumerable<CardModel> drawn = await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), Amount, player);
        foreach (CardModel card in drawn)
        {
            if (!dazedTemplate.CanEnchant(card))
            {
                continue;
            }

            EnchantmentModel dazed = ModelDb.Enchantment<Dazed>().ToMutable();
            CardCmd.Enchant(dazed, card, amount: 1m);
        }
    }
}
