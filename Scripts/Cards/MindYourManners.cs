using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>不得无礼！：将你的易伤、虚弱、脆弱转移至目标，获得格挡（9，升级 12）。</summary>
[RegisterCard(typeof(QueenCardPool))]
public sealed class MindYourManners : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(9m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<VulnerablePower>(),
		HoverTipFactory.FromPower<WeakPower>(),
		HoverTipFactory.FromPower<FrailPower>(),
	];

	public MindYourManners()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

		Creature self = base.Owner.Creature;
		Creature enemy = cardPlay.Target;

		await TransferPower<VulnerablePower>(choiceContext, self, enemy);
		await TransferPower<WeakPower>(choiceContext, self, enemy);
		await TransferPower<FrailPower>(choiceContext, self, enemy);

		await CreatureCmd.GainBlock(self, base.DynamicVars.Block, cardPlay);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(3m);
	}

	private async Task TransferPower<T>(PlayerChoiceContext choiceContext, Creature from, Creature to)
		where T : PowerModel
	{
		T? power = from.GetPower<T>();
		if (power == null || power.Amount <= 0m)
		{
			return;
		}

		decimal amount = power.Amount;
		await PowerCmd.Remove(power);
		await PowerCmd.Apply<T>(choiceContext, to, amount, from, this);
	}
}
