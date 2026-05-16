using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

public sealed class Toxic : QueenEnchantmentModel
{
    public override bool ShowAmount => true;
    public override bool HasExtraCardText => true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card))
        {
            return false;
        }
        return card.Enchantment is null;
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        _ = cardPlay;

        if (base.Card?.CombatState is not { } combatState || base.Card.Owner == null)
        {
            return;
        }

        List<Creature> enemies = combatState.HittableEnemies.ToList();
        if (enemies.Count == 0)
        {
            return;
        }

        if (base.Card.Owner.RunState.Rng.CombatCardSelection.NextItem(enemies) is not { } target)
        {
            return;
        }

        await PowerCmd.Apply<PoisonPower>(choiceContext, target, base.Amount, base.Card.Owner.Creature, base.Card);
    }
}

