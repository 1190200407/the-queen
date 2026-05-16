using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

/// <summary>臣服于我！：基础伤害 + 目标每有 1 点净失去的力量（StrengthPower 为负）�?ExtraDamage�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class SubmitToMe : QueenCardModel
{
	private const int energyCost = 2;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Common;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new CalculationBaseVar(10m),
		new ExtraDamageVar(3m),
		new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) =>
		{
			_ = card;
			decimal amt = target?.GetPower<StrengthPower>()?.Amount ?? 0m;
			return amt < 0m ? -amt : 0m;
		}),
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

	public SubmitToMe()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		if (!cardPlay.Target.IsAlive)
		{
			return;
		}

		await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.CalculationBase.UpgradeValueBy(4m);
		base.DynamicVars.ExtraDamage.UpgradeValueBy(1m);
	}
}
