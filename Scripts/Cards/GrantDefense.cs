using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class GrantDefense : QueenCardModel
{
	private const decimal learnIntentBlock = 4m;
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Common;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new SummonVar(3m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
		new AmalgamLearnIntentBlockVar(learnIntentBlock)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [QueenHoverTips.LearnIntent];

	public GrantDefense()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = cardPlay;
		await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);
		decimal blockBase = base.DynamicVars["LearnIntentBlock"].BaseValue;
		AmalgamActionModel? intent = AmalgamActionRegistry.CreateBlock(blockBase);
		await FriendlyAmalgamCmd.LearnIntent(choiceContext, base.Owner, intent, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Summon.UpgradeValueBy(2m);
		base.DynamicVars["LearnIntentBlock"].UpgradeValueBy(2m);
	}
}
