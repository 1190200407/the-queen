using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;


[RegisterCard(typeof(QueenCardPool))]
public sealed class GemWhirl : QueenCardModel, ISoulLampEventListener
{
	private bool _soulLampReturnFromExhaustPending = true;

	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [ModKeywordRegistry.GetCardKeyword(QueenKeyword.Fade)];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Draw", 1m)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
		HoverTipFactory.FromPower<SoulLampPower>(),
		ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade)
	];

	public GemWhirl()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	public override Task BeforeCombatStart()
	{
		_soulLampReturnFromExhaustPending = true;
		return Task.CompletedTask;
	}

	public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner)
		{
			_soulLampReturnFromExhaustPending = true;
		}

		return Task.CompletedTask;
	}

	public async Task OnSoulLampAmountChanged(
		PlayerChoiceContext choiceContext,
		Player player,
		decimal delta,
		Creature? applier,
		CardModel? cardSource)
	{
		_ = choiceContext;
		_ = applier;
		_ = cardSource;
		if (delta <= 0m || !_soulLampReturnFromExhaustPending || base.Owner != player || base.Pile?.Type != PileType.Exhaust)
		{
			return;
		}

		_soulLampReturnFromExhaustPending = false;
		await CardPileCmd.Add(this, PileType.Hand);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CardPileCmd.Draw(choiceContext, base.DynamicVars["Draw"].BaseValue, base.Owner);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["Draw"].UpgradeValueBy(1m);
	}
}
