using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(QueenCardPool))]
public sealed class SoulScattered : QueenCardModel, ICanMonsterCapture
{
	private const int energyCost = 1;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public bool CanCapture(MonsterModel monster, ICombatState combatState) =>
		monster is not null && combatState is not null
		&& MonsterCaptureRewardCatalog.GetEncounterRoomType(combatState) != RoomType.Boss;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(15m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.Static(StaticHoverTip.Fatal),
		QueenHoverTips.Capture,
	];

	public SoulScattered()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		Creature target = cardPlay.Target;
		bool shouldTriggerFatal = target.Powers.All(static p => p.ShouldOwnerDeathTriggerFatal());

		AttackCommand attackCommand = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay)
			.Targeting(target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);

		if (base.CombatState is not { } combatState ||
		    combatState.RunState.CurrentRoom is not CombatRoom combatRoom ||
		    target.Monster is null ||
		    !CanCapture(target.Monster, combatState))
		{
			return;
		}

		if (!shouldTriggerFatal || !QueenDamageResults.AnyTargetKilled(attackCommand))
		{
			return;
		}

		foreach (Player player in combatState.Players)
		{
			CardModel? reward = MonsterCaptureRewardCatalog.TryCreateCaptureRewardCard(player, target);
			if (reward is not { } rewardCard)
			{
				continue;
			}

			combatRoom.AddExtraReward(player, new SpecialCardReward(rewardCard, player));
			await CaptureSuccessPower.ApplyForCapture(player, rewardCard, this);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(5m);
	}
}
