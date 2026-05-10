using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>撕裂空间：仅单人可出现；场上负面状态（非临时减益能力）数量大于 10 时可打出；对所有敌人造成伤害。</summary>
[Pool(typeof(QueenCardPool))]
public sealed class TearSpace : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.AllEnemies;
	private const bool shouldShowInCardLibrary = true;

	/// <summary>场上负面数量须<strong>超过</strong>该值才可打出（即至少 11 个计数）。</summary>
	private const int maxDebuffCountToRemainUnplayable = 10;

	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(50m, ValueProp.Move),
		new CalculationBaseVar(0m),
		new CalculationExtraVar(1m),
		new CalculatedVar("BattlefieldDebuffCount").WithMultiplier(static (CardModel card, Creature? _) =>
		{
			CombatState? cs = card.Owner?.Creature?.CombatState;
			return cs == null ? 0m : CountBattlefieldDebuffs(cs);
		}),
	];

	protected override bool IsPlayable
	{
		get
		{
			CombatState? combatState = base.Owner?.Creature?.CombatState;
			if (combatState == null || base.Owner == null)
			{
				return true;
			}

			return CountBattlefieldDebuffs(combatState) >= maxDebuffCountToRemainUnplayable;
		}
	}

	public TearSpace()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = cardPlay;
		if (base.CombatState is not CombatState combatState)
		{
			return;
		}

		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.TargetingAllOpponents(combatState)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(13m);
	}

	private static int CountBattlefieldDebuffs(CombatState combatState)
	{
		int n = 0;
		foreach (Creature creature in combatState.Creatures)
		{
			if (!creature.IsAlive)
			{
				continue;
			}

			foreach (PowerModel power in creature.Powers)
			{
				if (power.Type == PowerType.Debuff)
				{
					n++;
				}
			}
		}

		return n;
	}
}
