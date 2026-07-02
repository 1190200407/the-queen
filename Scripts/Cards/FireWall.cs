using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;


[RegisterCard(typeof(QueenCardPool))]
public sealed class FireWall : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public override bool GainsBlock => true;

<<<<<<< HEAD
	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(4m, ValueProp.Move)];
=======
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new BlockVar(4m, ValueProp.Move),
		new CalculationBaseVar(0m),
		new CalculationExtraVar(4m),
		new CalculatedBlockVar(ValueProp.Move).WithMultiplier(static (CardModel card, Creature? _) =>
		{
			SoulLampPower? lamp = card.Owner?.Creature?.GetPower<SoulLampPower>();
			return lamp?.DisplayAmount ?? 0m;
		}),
	];
>>>>>>> beta

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<SoulLampPower>()];

	public FireWall()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await QueenCardCmd.AddSoulLamp(choiceContext, base.Owner, 1);

		decimal block = base.DynamicVars.CalculatedBlock.Calculate(cardPlay.Target);
		if (block <= 0m)
		{
			return;
		}

<<<<<<< HEAD
		decimal total = base.DynamicVars.Block.BaseValue * stacks;
		await CreatureCmd.GainBlock(base.Owner.Creature, total, ValueProp.Move, cardPlay);
=======
		await CreatureCmd.GainBlock(
			base.Owner.Creature,
			block,
			base.DynamicVars.CalculatedBlock.Props,
			cardPlay);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(1m);
		base.DynamicVars.CalculationExtra.UpgradeValueBy(1m);
>>>>>>> beta
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(1m);
	}
}
