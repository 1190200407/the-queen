using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>压迫感：造成基础伤害；目标本场战斗每失去过 1 点力量，额外造成伤害。</summary>
[RegisterCard(typeof(QueenCardPool))]
public sealed class OppressivePresence : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new CalculationBaseVar(5m),
		new ExtraDamageVar(3m),
		new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (_, target) => GetTargetStrengthLost(target)),
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

	public OppressivePresence()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
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
		base.DynamicVars.CalculationBase.UpgradeValueBy(1m);
		base.DynamicVars.ExtraDamage.UpgradeValueBy(1m);
	}

	private static decimal GetTargetStrengthLost(Creature? target)
	{
		if (target == null || !CombatManager.Instance.IsInProgress)
		{
			return 0m;
		}

		return CombatManager.Instance.History.Entries
			.OfType<PowerReceivedEntry>()
			.Where(e => e.Actor == target && e.Power is StrengthPower && e.Amount < 0m)
			.Sum(e => -e.Amount);
	}
}
