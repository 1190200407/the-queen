using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

using STS2RitsuLib.Keywords;
namespace ComicChess.TheQueen;

/// <summary>喂食根骨：消耗1张牌，聚合体获得力量。消逝、魂缚（<see cref="HasSelfBound"/>）。</summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class FeedingBone : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Token;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = false;

	public override int MaxUpgradeLevel => 1;

	internal override bool HasSelfBound => true;

	protected override IEnumerable<string> RegisteredKeywordIds => [QueenKeyword.Fade];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(1m)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<StrengthPower>(),
		ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade)
	];

	public FeedingBone()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = cardPlay;
		IICombatState? cs = base.Owner.Creature.CombatState;
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

		Creature? amalgam = FriendlyAmalgamCmd.GetExisting(cs, base.Owner);
		if (amalgam is { IsAlive: true })
		{
			await PowerCmd.Apply<StrengthPower>(choiceContext, 
				amalgam,
				base.DynamicVars.Strength.BaseValue,
				base.Owner.Creature,
				this);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Strength.UpgradeValueBy(1m);
	}
}
