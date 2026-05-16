using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Keywords;
namespace ComicChess.TheQueen;


public sealed class HandOfSeizure : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
		ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade),
		HoverTipFactory.FromCard<HandOfRefusal>(upgrade: base.IsUpgraded),
		.. HoverTipFactory.FromAffliction<Bound>(),
		HoverTipFactory.FromPower<SoulLampPower>()
	];

	internal override bool HasSelfBound => true;

	public HandOfSeizure()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Target != null)
		{
			await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
				.FromCard(this)
				.Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_blunt")
				.Execute(choiceContext);
		}

		HandOfRefusalNextTurnPower? handsNextTurn = await PowerCmd.Apply<HandOfRefusalNextTurnPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		if (handsNextTurn != null)
		{
			handsNextTurn.isUpgraded = base.IsUpgraded;
		}
		await QueenCardCmd.AddSoulLamp(base.Owner);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}
}
