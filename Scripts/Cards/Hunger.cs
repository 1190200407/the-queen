using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class Hunger : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Power;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromCard<Devour>(upgrade: base.IsUpgraded),
		HoverTipFactory.FromPower<HungerPower>(),
		QueenHoverTips.SoulLamp
	];

	public Hunger()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.CombatState == null || base.Owner == null)
		{
			return;
		}

		await QueenCardCmd.CreateInHand<Devour>(base.Owner, base.CombatState, base.IsUpgraded, isBounded: true);
		await QueenCardCmd.AddSoulLamp(base.Owner, 1);
		await PowerCmd.Apply<HungerPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
	}
}
