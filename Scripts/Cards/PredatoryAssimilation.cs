using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rooms;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>�?��?�?�??�?�?��??�??�?�?�?�?�?��?�?��??�??精�?��??�?��?�可�?�?��?�?��?�??<see cref="FriendlyAmalgamHook"/>�?�??/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class PredatoryAssimilation : QueenCardModel, ICanMonsterCapture
{
	private const int energyCost = 1;
	private const CardType type = CardType.Power;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	public bool CanCapture(MonsterModel monster, CombatState combatState) =>
		monster is not null && combatState is not null
		&& (combatState.Encounter?.RoomType switch
		{
			RoomType.Boss => false,
			RoomType.Elite => IsUpgraded,
			_ => true,
		});

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.Static(StaticHoverTip.Fatal),
		QueenHoverTips.Capture
	];

	public PredatoryAssimilation()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = choiceContext;
		_ = cardPlay;

		if (base.Owner.Creature.GetPower<PredatoryAssimilationPower>() is PredatoryAssimilationPower power && !power.IsUpgraded && base.IsUpgraded)
		{
			power.ConfigureIsUpgraded(base.IsUpgraded, true);
			return;
		}

		PredatoryAssimilationPower? predatoryAssimilation = await PowerCmd.Apply<PredatoryAssimilationPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		if (predatoryAssimilation is not null)
		{
			predatoryAssimilation.ConfigureIsUpgraded(base.IsUpgraded);
		}

	}
}
