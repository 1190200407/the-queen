using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>精神控制：少量伤害；指定敌人下一次对你的怪物攻击改打别处；消耗�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class MindControl : QueenCardModel
{
	private const int energyCost = 3;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(1m, ValueProp.Move)];

	public MindControl()
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

		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);

		if (base.Owner.Creature is not { } queen)
		{
			return;
		}

		_ = await PowerCmd.Apply<MindControlPower>(cardPlay.Target, 1m, queen, this);
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
