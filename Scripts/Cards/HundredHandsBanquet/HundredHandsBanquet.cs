using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(QueenCardPool))]
public sealed class HundredHandsBanquet : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override bool HasEnergyCostX => true;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromCard<HandOfSeizure>(upgrade: base.IsUpgraded),
		HoverTipFactory.FromCard<HandOfRefusal>(upgrade: base.IsUpgraded),
		HoverTipFactory.FromPower<SoulLampPower>(),
		.. HoverTipFactory.FromAffliction<Bound>(),
	];

	public HundredHandsBanquet()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.CombatState == null || base.Owner == null)
		{
			return;
		}

		await CreateInHandInternal(base.Owner, base.CombatState, base.IsUpgraded);

		int soulLampGain = ResolveEnergyXValue();
		if (soulLampGain > 0)
		{
			await QueenCardCmd.AddSoulLamp(choiceContext, base.Owner, soulLampGain);
		}
	}

	internal static async Task CreateInHandInternal(Player owner, CombatState? combatState, bool isUpgraded = false)
	{
		if (combatState == null)
		{
			return;
		}

		await QueenCardCmd.CreateInHand<HandOfSeizure>(owner, combatState, isUpgraded);
		await QueenCardCmd.CreateInHand<HandOfRefusal>(owner, combatState, isUpgraded);
	}
}
