using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

using STS2RitsuLib.Cards.DynamicVars;

using STS2RitsuLib.Keywords;
namespace ComicChess.TheQueen;

/// <summary>喂食血肉：消耗1张牌，召唤。消逝、魂缚（<see cref="HasSelfBound"/>）。</summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class FeedingFlesh : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Token;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = false;

	public override int MaxUpgradeLevel => 1;

	internal override bool HasSelfBound => true;

	protected override IEnumerable<string> RegisteredKeywordIds => [QueenKeyword.Fade];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new SummonVar(5m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC")];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade)];

	public FeedingFlesh()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = cardPlay;
		CombatState? cs = base.Owner.Creature.CombatState;
		if (cs == null)
		{
			return;
		}

		IEnumerable<CardModel> pick = await CardSelectCmd.FromHand(
			choiceContext,
			base.Owner,
			new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
			null,
			this);
		CardModel? toEx = pick.FirstOrDefault();
		if (toEx == null)
		{
			return;
		}

		await CardCmd.Exhaust(choiceContext, toEx);
		await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Summon.UpgradeValueBy(2m);
	}
}
