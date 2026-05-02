using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class OppressivePresence : QueenCardModel
{
	/// <summary>本场战斗中每名敌人 <see cref="StrengthPower"/> 累计减少量；每场战斗开始时清空。</summary>
	private Dictionary<Creature, decimal> StrengthLossByEnemy = new();
	private const int energyCost = 1;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new CalculationBaseVar(5m),
		new ExtraDamageVar(3m),
		new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (CardModel card, Creature? target) =>
			(card as OppressivePresence)?.StrengthLossRecordedFor(target) ?? 0m),
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

	public OppressivePresence()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	public override Task AfterCardEnteredCombat(CardModel card)
	{
		if (card == this && !card.IsClone)
		{
			MergeStrengthLossFromHistory();
		}
		return Task.CompletedTask;
	}

	public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		_ = applier;
		_ = cardSource;
		if (power is not StrengthPower || amount >= 0m)
		{
			return Task.CompletedTask;
		}

		Creature? strOwner = power.Owner;
		if (strOwner is not { IsMonster: true })
		{
			return Task.CompletedTask;
		}

		decimal loss = -amount;
		if (StrengthLossByEnemy.TryGetValue(strOwner, out decimal sum))
		{
			StrengthLossByEnemy[strOwner] = sum + loss;
		}
		else
		{
			StrengthLossByEnemy[strOwner] = loss;
		}

		return Task.CompletedTask;
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

		await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.CalculationBase.UpgradeValueBy(2m);
		base.DynamicVars.ExtraDamage.UpgradeValueBy(1m);
	}

	private decimal StrengthLossRecordedFor(Creature? enemy) =>
		enemy is null ? 0m : StrengthLossByEnemy.GetValueOrDefault(enemy);

	private void MergeStrengthLossFromHistory()
	{
		StrengthLossByEnemy.Clear();
		foreach (CombatHistoryEntry entry in CombatManager.Instance.History.Entries)
		{
			if (entry is not PowerReceivedEntry pr || pr.Amount >= 0m)
			{
				continue;
			}

			if (pr.Power is not StrengthPower)
			{
				continue;
			}

			Creature strOwner = pr.Actor;
			if (strOwner is not { IsMonster: true })
			{
				continue;
			}

			decimal loss = -pr.Amount;
			if (StrengthLossByEnemy.TryGetValue(strOwner, out decimal sum))
			{
				StrengthLossByEnemy[strOwner] = sum + loss;
			}
			else
			{
				StrengthLossByEnemy[strOwner] = loss;
			}
		}
	}
}
