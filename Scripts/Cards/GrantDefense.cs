using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Cards.DynamicVars;

namespace ComicChess.TheQueen;


public sealed class GrantDefense : LearnIntentCardModel
{
	private const decimal learnIntentBlock = 4m;
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Common;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new SummonVar(3m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
		new AmalgamLearnIntentBlockVar(learnIntentBlock)
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [QueenHoverTips.LearnIntent];
	protected override bool ShouldSummonBeforeLearnIntent => true;

	public GrantDefense()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = choiceContext;
		_ = cardPlay;
		decimal blockBase = base.DynamicVars["LearnIntentBlock"].BaseValue;
		AmalgamActionModel? intent = AmalgamActionRegistry.CreateBlock(blockBase);
		return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Summon.UpgradeValueBy(2m);
		base.DynamicVars["LearnIntentBlock"].UpgradeValueBy(2m);
	}
}
