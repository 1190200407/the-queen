using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class HundredHandsBanquet : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override bool HasEnergyCostX => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromCard<BloodthirstScratch>(upgrade: base.IsUpgraded),
		.. HoverTipFactory.FromAffliction<Bound>()
	];

	public HundredHandsBanquet()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.CombatState == null || base.Owner?.Creature == null)
		{
			return;
		}

		int x = ResolveEnergyXValue();
		if (x <= 0)
		{
			return;
		}

		for (int i = 0; i < x; i++)
		{
			await QueenCardCmd.CreateInHand<BloodthirstScratch>(base.Owner, base.CombatState, base.IsUpgraded);
		}

		await QueenCardCmd.AddSoulLamp(base.Owner, x);
	}
}
