using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

public sealed class LifeArtPower : QueenPowerModel
{
	private sealed class Data
	{
		public readonly HashSet<Player> TriggeredPlayers = [];
	}

	protected override object? InitInternalData() => new Data();

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;
	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("CreateUpgradedDevour", 0)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromCard<Devour>(base.DynamicVars["CreateUpgradedDevour"].BaseValue > 0m),
		HoverTipFactory.FromPower<SoulLampPower>(),
	];

	public override Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		// 避免降级
		if (base.DynamicVars["CreateUpgradedDevour"].BaseValue == 0m)
			base.DynamicVars["CreateUpgradedDevour"].BaseValue = cardSource is { IsUpgraded: true } ? 1m : 0m;
		return base.BeforeApplied(target, amount, applier, cardSource);
	}

	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		_ = cardSource;
		if (amount <= 0m || !QueenDebuffUtil.IsDebuff(power))
		{
			return;
		}

		Player? owner = base.Owner.Player;
		Player? applierOwner = ResolvePlayerOwner(applier);
		if (owner == null || applierOwner == null || applierOwner == owner)
		{
			return;
		}

		if (!GetInternalData<Data>().TriggeredPlayers.Add(applierOwner))
		{
			return;
		}

		if (base.CombatState is not { } combatState)
		{
			return;
		}

		Flash();
		bool createUpgradedDevour = base.DynamicVars["CreateUpgradedDevour"].BaseValue > 0m;
		int repeats = (int)Amount;
		for (int i = 0; i < repeats; i++)
		{
			await QueenCardCmd.CreateInHand<Devour>(owner, combatState, createUpgradedDevour);
			await QueenCardCmd.AddSoulLamp(choiceContext, owner, 1);
		}
	}

	public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		_ = choiceContext;
		GetInternalData<Data>().TriggeredPlayers.Remove(player);
		return Task.CompletedTask;
	}

	private static Player? ResolvePlayerOwner(Creature? creature) =>
		creature?.Player ?? creature?.PetOwner as Player;
}
