using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace ComicChess.TheQueen;

/// <summary>任性：回合开始从已解锁卡池随机生成一张牌入手并魂缚；若打出的是升级过的「任性」，则生成物为已升级形态。再获得魂灯。</summary>
public sealed class WillfulPower : QueenPowerModel
{
	private bool _grantUpgradedGeneratedCard;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;
	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	/// <summary>与掠食同化 <c>Upgraded</c> 同理：<c>IfUpgradedVar</c> 在 <c>DeepCloneFields</c> 时即入 DynamicVarSet，早于 <c>BeforeApplied</c> 会冻在 Normal。</summary>
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new IntVar("GrantUpgradedCard", 0),
	];

	public override Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		_grantUpgradedGeneratedCard = cardSource is { IsUpgraded: true };
		base.DynamicVars["GrantUpgradedCard"].BaseValue = _grantUpgradedGeneratedCard ? 1m : 0m;
		return base.BeforeApplied(target, amount, applier, cardSource);
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		_ = choiceContext;
		if (player != base.Owner.Player)
		{
			return;
		}

		CombatState? combatState = base.Owner.CombatState;
		if (combatState == null)
		{
			return;
		}
		
		IEnumerable<CardModel> forCombat = CardFactory.GetForCombat(player, from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
			select c, 1, player.RunState.Rng.CombatCardGeneration);
		foreach (CardModel item in forCombat)
		{
			item.SetToFreeThisCombat();
			await CardPileCmd.AddGeneratedCardToCombat(item, PileType.Hand, player);
			if (_grantUpgradedGeneratedCard && item.IsUpgradable && !item.IsUpgraded)
			{
				CardCmd.Upgrade(item, CardPreviewStyle.None);
			}
			if (item.Affliction is not null && item.Affliction is not Bound)
			{
				CardCmd.ClearAffliction(item);
			}
			if (item.Affliction is not Bound)
			{
				await CardCmd.Afflict<Bound>(item, 1m);
			}
		}
		await QueenCardCmd.AddSoulLamp(choiceContext, player, 1);
	}
}
