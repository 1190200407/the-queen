using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;


public sealed class GemShield : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Common;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new BlockVar(7m, ValueProp.Move),
		new EnergyVar(1),
		new DynamicVar("SoulLampNextTurn", 0m)
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => IsUpgraded
		? [base.EnergyHoverTip, HoverTipFactory.FromPower<SoulLampPower>()]
		: [base.EnergyHoverTip];

	public GemShield()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		await PowerCmd.Apply<EnergyNextTurnPower>(base.Owner.Creature, base.DynamicVars.Energy.BaseValue, base.Owner.Creature, this);
		if (base.DynamicVars["SoulLampNextTurn"].BaseValue > 0m)
		{
			await PowerCmd.Apply<SoulLampNextTurnPower>(base.Owner.Creature, base.DynamicVars["SoulLampNextTurn"].BaseValue, base.Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(2m);
		base.DynamicVars["SoulLampNextTurn"].UpgradeValueBy(1m);
	}
}
