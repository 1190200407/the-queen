using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class SoulBeam : QueenCardModel
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

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [QueenHoverTips.LearnIntent];

	public SoulBeam()
		: base(0, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = cardPlay;
		int hits = ResolveEnergyXValue();
		if (base.IsUpgraded)
		{
			hits++;
		}

		decimal dmg = base.DynamicVars["LearnIntentDamage"].BaseValue;
		AmalgamActionModel? intent = AmalgamActionRegistry.CreateOffenseMulti(dmg, hits);
		await FriendlyAmalgamCmd.LearnIntent(choiceContext, base.Owner, intent, this);
	}
}
