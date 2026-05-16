using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>享用晚餐：造成伤害，在手牌中生成两张吞噬（升级后为吞噬+）�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class EnjoyDinner : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Common;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromCard<Devour>(base.IsUpgraded),
		HoverTipFactory.FromPower<SoulLampPower>(),
	];

	public EnjoyDinner()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

		if (base.CombatState == null || base.Owner == null)
		{
			return;
		}

		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);

		for (int i = 0; i < 2; i++)
		{
			await QueenCardCmd.CreateInHand<Devour>(base.Owner, base.CombatState, base.IsUpgraded);
		}

		await QueenCardCmd.AddSoulLamp(base.Owner, 2);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
	}
}
