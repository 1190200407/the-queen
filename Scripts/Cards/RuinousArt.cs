using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(QueenCardPool))]
public sealed class RuinousArt : QueenCardModel
{
	private const int energyCost = 5;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(24m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		base.EnergyHoverTip,
		HoverTipFactory.FromPower<SoulLampPower>(),
		.. HoverTipFactory.FromAffliction<Bound>(),
	];

	public RuinousArt()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		ArgumentNullException.ThrowIfNull(base.Owner.PlayerCombatState, nameof(base.Owner.PlayerCombatState));

		List<CardModel> handCards = base.Owner.PlayerCombatState.Hand.Cards.ToList();
		int soulLampGain = handCards.Count;

		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);

		foreach (CardModel card in handCards)
		{
			CardCmd.ClearAffliction(card);
			await CardCmd.Afflict<Bound>(card, 1m);
		}

		if (soulLampGain > 0)
		{
			await QueenCardCmd.AddSoulLamp(choiceContext, base.Owner, soulLampGain);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(6m);
	}
}
