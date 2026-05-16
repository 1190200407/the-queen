using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>喂食：生成喂食根骨、喂食血肉，获得魂灯。衍生牌为消�?魂缚�?see cref="HasSelfBound"/>）�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class Feeding : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public override int MaxUpgradeLevel => 1;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromCard<FeedingBone>(base.IsUpgraded),
		HoverTipFactory.FromCard<FeedingFlesh>(base.IsUpgraded),
		HoverTipFactory.FromPower<SoulLampPower>()
	];

	public Feeding()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = cardPlay;
		if (base.CombatState == null)
		{
			return;
		}

		bool up = base.IsUpgraded;
		await QueenCardCmd.CreateInHand<FeedingBone>(base.Owner, base.CombatState, isUpgraded: up);
		await QueenCardCmd.CreateInHand<FeedingFlesh>(base.Owner, base.CombatState, isUpgraded: up);
		await QueenCardCmd.AddSoulLamp(base.Owner, 1);
	}

	/// <summary>升级只影响衍生的根骨+ / 血�?，本体数值不变�?/summary>
	protected override void OnUpgrade()
	{
	}
}
