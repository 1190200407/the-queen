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
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;


public sealed class Joyless : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(3m, ValueProp.Move),
		new RepeatVar(2)
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

	public Joyless()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

		for (int i = 0; i < base.DynamicVars.Repeat.IntValue; i++)
		{
			await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
				.FromCard(this)
				.Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_blunt")
				.Execute(choiceContext);
		}

		Player? owner = base.Owner;
		CombatState? combat = base.CombatState;
		if (owner == null || combat == null)
		{
			return;
		}

		IEnumerable<CardModel> selected = await CardSelectCmd.FromHandForDiscard(
			choiceContext,
			owner,
			new CardSelectorPrefs(new LocString("cards", "COMICCHESS-JOYLESS.selectionPrompt"), 0, 999999999),
			c => c != this,
			this);

		// DiscardAndDraw 用首牌解析 CombatState；若 card.CombatState 与 Owner.Creature.CombatState 皆为 null，
		// 会在 History / Hook 里对 null combatState 访问 RoundNumber 而 NRE（Boss 战等偶发）。
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
		await PowerCmd.Apply<JoylessStrengthPower>(owner.Creature, toDiscard.Count, owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
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
