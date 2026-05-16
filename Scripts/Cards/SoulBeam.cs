using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;


public sealed class SoulBeam : LearnIntentCardModel
{
	private const decimal learnIntentDamagePerHit = 8m;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override bool HasEnergyCostX => true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new AmalgamLearnIntentDamageVar(learnIntentDamagePerHit, ValueProp.Move)
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [QueenHoverTips.LearnIntent];

	public SoulBeam()
		: base(0, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = choiceContext;
		_ = cardPlay;
		int hits = ResolveEnergyXValue();
		if (base.IsUpgraded)
		{
			hits++;
		}

		decimal dmg = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
		AmalgamActionModel? intent = AmalgamActionRegistry.CreateOffenseMulti(dmg, hits);
		return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
	}
}
