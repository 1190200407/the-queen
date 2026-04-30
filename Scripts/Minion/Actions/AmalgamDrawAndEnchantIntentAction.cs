using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：抽牌后为抽到的牌附魔（若可附魔）。</summary>
public sealed class AmalgamDrawAndEnchantIntentAction<TEnchantment> : AmalgamActionModel
	where TEnchantment : EnchantmentModel
{
	public AmalgamDrawAndEnchantIntentAction(decimal drawCount)
		: base(drawCount)
	{
	}

	public override LocString IntentTitle => new("monsters", "FRIENDLY_AMALGAM.intent_draw.title");

	public override LocString GetIntentDescription()
	{
		var desc = new LocString("monsters", "FRIENDLY_AMALGAM.intent_draw.description");
		desc.Add("Amount", Amount);
		return desc;
	}

	protected override MoveState CreateMoveState()
	{
		// NOTE: intent label/description is handled by the intent itself (intents table).
		// We keep MoveState key unique by enchantment type to avoid collisions.
		return new MoveState(
			$"AMALGAM_INTENT_DRAW_ENCHANT_{typeof(TEnchantment).Name}",
			_ => Task.CompletedTask,
			new AmalgamDrawAndEnchantIntent(Amount, ModelDb.Enchantment<TEnchantment>().Id.Entry));
	}

	protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
	{
		if (amalgam.PetOwner is not { Creature: { } ownerCreature } || !ownerCreature.IsAlive)
		{
			return;
		}

		Player? owner = ownerCreature.Player;
		if (owner == null)
		{
			return;
		}

		IEnumerable<CardModel> drawn = await CardPileCmd.Draw(choiceContext, Amount, owner);
		foreach (CardModel card in drawn)
		{
			EnchantmentModel enchantment = ModelDb.Enchantment<TEnchantment>().ToMutable();
			if (!enchantment.CanEnchant(card))
			{
				continue;
			}

			CardCmd.Enchant(enchantment, card, amount: 1m);
		}
	}
}

