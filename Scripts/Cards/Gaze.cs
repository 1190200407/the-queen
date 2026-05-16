using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>注视：本回合与下回合各使目标失去 2 点力量；基础消耗�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class Gaze : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Common;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;
	private const decimal strengthLoss = 2m;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<StrengthPower>()
	];

	public Gaze()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = choiceContext;
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		Creature target = cardPlay.Target;
		if (!target.IsAlive || base.Owner.Creature is not { IsAlive: true } applier)
		{
			return;
		}

		await PowerCmd.Apply<GazeEnemyStrengthPower>(target, strengthLoss, applier, this);
		await PowerCmd.Apply<GazeNextTurnStrengthLossPower>(target, strengthLoss, applier, this);
	}

	protected override void OnUpgrade()
	{
		RemoveKeyword(CardKeyword.Exhaust);
	}
}
