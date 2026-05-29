using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(QueenCardPool))]
public sealed class Scratch : QueenCardModel
{
	private const int ScratchBaseHitCount = 1;

	private const int energyCost = 0;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Common;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	private int _extraHitCountFromScratchPlays;

	private int ExtraHitCountFromScratchPlays
	{
		get => _extraHitCountFromScratchPlays;
		set
		{
			AssertMutable();
			_extraHitCountFromScratchPlays = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(5m, ValueProp.Move),
		new RepeatVar(1),
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		.. HoverTipFactory.FromAffliction<Bound>(),
	];

	internal override bool HasSelfBound => true;

	public Scratch()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	public override async Task AfterCardEnteredCombat(CardModel card)
	{
		await base.AfterCardEnteredCombat(card);
		if (card != this || base.IsClone)
		{
			return;
		}

		SyncExtraHitsFromHistory();
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.WithHitCount(base.DynamicVars.Repeat.IntValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);

		ArgumentNullException.ThrowIfNull(base.Owner);
		ArgumentNullException.ThrowIfNull(base.Owner.PlayerCombatState);
		foreach (Scratch scratch in base.Owner.PlayerCombatState.AllCards.OfType<Scratch>())
		{
			scratch.BuffHitCountFromScratchPlay(1);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
	}

	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		SyncRepeatVarToCombatHits();
	}

	private void SyncExtraHitsFromHistory()
	{
		ExtraHitCountFromScratchPlays = CountScratchPlaysThisCombat(base.Owner);
		SyncRepeatVarToCombatHits();
	}

	internal void BuffHitCountFromScratchPlay(int delta = 1)
	{
		ExtraHitCountFromScratchPlays += delta;
		SyncRepeatVarToCombatHits();
	}

	private void SyncRepeatVarToCombatHits()
	{
		base.DynamicVars.Repeat.BaseValue = ScratchBaseHitCount + ExtraHitCountFromScratchPlays;
	}

	private int CountScratchPlaysThisCombat(Player? owner)
	{
		if (owner == null)
		{
			return 0;
		}

		return CombatManager.Instance.History.CardPlaysFinished.Count(e =>
			e.CardPlay.Card is Scratch && e.CardPlay.Card.Owner == owner);
	}
}
