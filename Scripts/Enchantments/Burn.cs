using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

public sealed class Burn : QueenEnchantmentModel
{
    public override bool ShowAmount => true;
    public override bool HasExtraCardText => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Unplayable)];

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card))
        {
            return false;
        }
        return card.Enchantment is null;
    }

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player)
        {
            return;
        }

        if (base.Card?.Pile?.Type != PileType.Hand)
        {
            return;
        }

        if (base.Card.CombatState is not { } combatState || base.Card.Owner == null)
        {
            return;
        }

        List<Creature> enemies = combatState.HittableEnemies.ToList();
        if (enemies.Count == 0)
        {
            return;
        }
        
		SfxCmd.Play("event:/sfx/characters/attack_fire");

        foreach (Creature enemy in enemies)
        {
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(enemy));
        }

        await CreatureCmd.Damage(
            choiceContext,
            enemies,
            base.Amount,
            ValueProp.Unpowered | ValueProp.SkipHurtAnim,
            base.Card.Owner.Creature,
            base.Card);
    }
}

