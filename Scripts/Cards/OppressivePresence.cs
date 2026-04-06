using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class OppressivePresence : QueenCardModel
{
	private const int energyCost = 3;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(3m, ValueProp.Move),
		new IntVar("Hits", 5m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromCard<Devour>(),
		.. HoverTipFactory.FromAffliction<Bound>()
	];

	internal override bool UseBoundAfflictionOverlayForPreview => true;

	public OppressivePresence()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	public override async Task BeforeCombatStart()
	{
		if (base.Owner?.Creature?.CombatState == null)
		{
			return;
		}
		if (Affliction is not Bound)
		{
			await CardCmd.Afflict<Bound>(this, 1m);
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.CombatState == null || base.Owner.PlayerCombatState == null)
		{
			return;
		}

		List<CardModel> devoursInHand = base.Owner.PlayerCombatState.Hand.Cards.Where(static c => c is Devour).ToList();
		bool skipPlayPileVfx = false;
		foreach (CardModel card in devoursInHand)
		{
			if (card.Pile?.Type != PileType.Hand)
			{
				continue;
			}

			await CardCmd.AutoPlay(
				choiceContext,
				card,
				target: null,
				AutoPlayType.Default,
				skipXCapture: false,
				skipCardPileVisuals: skipPlayPileVfx);
			skipPlayPileVfx = true;
		}

		decimal hitDamage = base.DynamicVars.Damage.BaseValue;
		int hits = base.DynamicVars["Hits"].IntValue;
		for (int i = 0; i < hits; i++)
		{
			List<Creature> enemies = base.CombatState.HittableEnemies.ToList();
			if (enemies.Count == 0)
			{
				break;
			}

			if (base.Owner.RunState.Rng.CombatCardSelection.NextItem(enemies) is not { } target)
			{
				break;
			}

			await DamageCmd.Attack(hitDamage)
				.FromCard(this)
				.Targeting(target)
				.WithHitFx("vfx/vfx_attack_blunt")
				.Execute(choiceContext);
		}
	}

	public override async Task AfterCardEnteredCombat(CardModel card)
	{
		if (card == this && Affliction is not Bound)
		{
			await CardCmd.Afflict<Bound>(this, 1m);
		}
	}
}
