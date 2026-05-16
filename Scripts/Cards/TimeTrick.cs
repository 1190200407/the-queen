using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>时间把戏：造成伤害；至多弃 2 张；下回合开始时将本次弃掉的牌移回手牌�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class TimeTrick : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	private const int maxDiscard = 2;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move)];

	public TimeTrick()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);

		Player? owner = base.Owner;
		CombatState? combat = base.CombatState;
		if (owner == null || combat == null)
		{
			return;
		}

		IEnumerable<CardModel> selected = await CardSelectCmd.FromHandForDiscard(
			choiceContext,
			owner,
			new CardSelectorPrefs(new LocString("cards", "COMICCHESS-TIME_TRICK.selectionPrompt"), 0, maxDiscard),
			c => c != this,
			this);

		IReadOnlyList<CardModel> handSnapshot = PileType.Hand.GetPile(owner).Cards;
		List<CardModel> toDiscard = selected
			.Distinct()
			.Where(c => IsValidHandDiscard(owner, combat, handSnapshot, c))
			.ToList();

		if (toDiscard.Count == 0)
		{
			return;
		}

		await CardCmd.Discard(choiceContext, toDiscard);

		TimeTrickReturnPendingPower? pending = owner.Creature.GetPower<TimeTrickReturnPendingPower>();
		if (pending != null)
		{
			foreach (CardModel c in toDiscard)
			{
				if (!pending.CardsToReturn.Contains(c))
				{
					pending.CardsToReturn.Add(c);
				}
			}
		}
		else
		{
			pending = await PowerCmd.Apply<TimeTrickReturnPendingPower>(choiceContext, owner.Creature, 1m, owner.Creature, this);
			pending?.CardsToReturn.AddRange(toDiscard);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(4m);
	}

	private static bool IsValidHandDiscard(
		Player expectedOwner,
		CombatState currentCombat,
		IReadOnlyList<CardModel> hand,
		CardModel? card)
	{
		if (card == null || !ReferenceEquals(card.Owner, expectedOwner) || card.Owner?.Creature == null)
		{
			return false;
		}

		if (!hand.Contains(card))
		{
			return false;
		}

		CombatState? resolved = card.Owner.Creature.CombatState ?? card.CombatState;
		return resolved != null && ReferenceEquals(resolved, currentCombat);
	}
}
