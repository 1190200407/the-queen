using System;
using System.Collections.Generic;
using System.Linq;
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
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>饕餮盛宴：自动打出消耗堆中所有 <see cref="Devour"/>，再对目标造成多次伤害（参考原版 <see cref="MegaCrit.Sts2.Core.Models.Cards.KnifeTrap"/>）。</summary>

public sealed class GluttonsFeast : QueenCardModel
{
	private const string CalculatedDevoursKey = "CalculatedDevours";

	internal override bool HasSelfBound => true;

	private const int energyCost = 1;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new CalculationBaseVar(0m),
		new CalculationExtraVar(1m),
		new CalculatedVar(CalculatedDevoursKey).WithMultiplier(static (CardModel card, Creature? _) =>
			card.Owner is null ? 0m : (decimal)PileType.Exhaust.GetPile(card.Owner).Cards.Count(static c => c is Devour)),
		new DamageVar(2m, ValueProp.Move),
		new RepeatVar(3),
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<Devour>()];

	public GluttonsFeast()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

		if (base.Owner is not Player owner || base.CombatState == null)
		{
			return;
		}

		//TODO 明确IEnumerablez在遍历时被修改的问题
		List<CardModel> devoursInExhaust = PileType.Exhaust.GetPile(owner).Cards.Where(static c => c is Devour).ToList();
		bool firstAutoPlay = true;
		for (int i = devoursInExhaust.Count - 1; i >= 0; i--)
		{
			await CardAutoPlayDirect.AutoPlayAsync(
				choiceContext,
				devoursInExhaust[i],
				cardPlay.Target,
				AutoPlayType.Default,
				skipXCapture: false,
				skipCardPileVisuals: !firstAutoPlay);
			firstAutoPlay = false;
		}

		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.WithHitCount(base.DynamicVars.Repeat.IntValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Repeat.UpgradeValueBy(1m);
	}
}
