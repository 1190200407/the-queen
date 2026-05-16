using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Creatures;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>脉冲打击�? 费罕见攻击；伤害；弃 2；每当手牌数达到 8 张或以上且本牌不在手牌时，移回手牌（与原版「如此甚好」同类钩子）�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class PulseStrike : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;
	private const int discardCount = 2;
	private const int handSizeToReturnSelf = 7;

	protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move)];

	public PulseStrike()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);

		Player? owner = base.Owner;
		if (owner == null)
		{
			return;
		}

		IEnumerable<CardModel> selected = await CardSelectCmd.FromHandForDiscard(
			choiceContext,
			owner,
			new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, discardCount),
			c => c != this,
			this);

		List<CardModel> toDiscard = selected.Distinct().ToList();
		if (toDiscard.Count > 0)
		{
			await CardCmd.Discard(choiceContext, toDiscard);
		}
	}

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		await TryReturnToHandIfEligible();
	}


    public override async Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
    {
		await TryReturnToHandIfEligible();
    }

    protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}

	private async Task TryReturnToHandIfEligible()
	{
		Player? owner = base.Owner;
		if (owner?.PlayerCombatState == null)
		{
			return;
		}

		if (base.Pile?.Type == PileType.Hand)
		{
			return;
		}

		if (owner.PlayerCombatState.Hand.Cards.Count <= handSizeToReturnSelf)
		{
			return;
		}

		await CardPileCmd.Add(this, PileType.Hand);
	}
}
