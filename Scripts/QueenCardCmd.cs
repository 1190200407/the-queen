using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;

namespace ComicChess.TheQueen;

public static class QueenCardCmd
{
    public static async Task CreateInHand<T>(Player owner, CombatState combatState, bool isUpgraded = false, bool isBounded = false) where T : CardModel
	{
        CardModel card = combatState.CreateCard<T>(owner);
        await CreateInHand(card, isUpgraded, isBounded);
	}

	public static async Task CreateInHand(CardModel card, bool isUpgraded = false, bool isBounded = false)
	{
        if (isUpgraded)
        {
            CardCmd.Upgrade(card);
        }

		if (card is ScratchTaggedCard scratchTagged)
		{
			Player? owner = card.Owner;
			if (owner != null)
			{
				QueenScratchBonusTracker.ApplyToNewScratchTagged(owner, scratchTagged);
			}
		}

		await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);
        if (isBounded)
        {
            await CardCmd.Afflict<Bound>(card, 1m);
        }
	}

    public static async Task AddSoulLamp(Player owner, int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        SoulLampPower? existing = owner.Creature.GetPower<SoulLampPower>();
        if (existing == null)
        {
            await PowerCmd.Apply<SoulLampPower>(owner.Creature, amount, owner.Creature, null);
            return;
        }

        if (existing.Amount <= 0)
        {
            // SoulLampPower uses -1 as the hidden "display 0" sentinel.
            // When gaining Soul Lamp from this state, jump directly to gained amount.
            await PowerCmd.SetAmount<SoulLampPower>(owner.Creature, amount, owner.Creature, null);
            return;
        }

        await PowerCmd.ModifyAmount(existing, amount, owner.Creature, null);
    }
}