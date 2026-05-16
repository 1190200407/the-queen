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


[RegisterCard(typeof(QueenCardPool))]
public sealed class BurnBrightForMe : QueenCardModel
{
	private const int energyCost = 2;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new PowerVar<StrengthPower>(1m),
		new BlockVar(13m, ValueProp.Move)
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

	public BurnBrightForMe()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = choiceContext;
		if (base.Owner.Creature.CombatState is { } combatState)
		{
			Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
			if (amalgam is { IsAlive: true })
			{
				await PowerCmd.Apply<StrengthPower>(
					amalgam,
					base.DynamicVars.Strength.BaseValue,
					base.Owner.Creature,
					this);
			}
		}

		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Strength.UpgradeValueBy(1m);
		base.DynamicVars.Block.UpgradeValueBy(5m);
	}
}
