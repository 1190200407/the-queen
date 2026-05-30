using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>战术指令：生成代劳、接手，获得魂灯；升级减费�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class TacticalDirective : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromCard<DelegateLabor>(),
		HoverTipFactory.FromCard<TakeOver>(),
		.. HoverTipFactory.FromAffliction<Bound>(),
		ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade),
		HoverTipFactory.FromPower<SoulLampPower>()
	];

	public TacticalDirective()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = choiceContext;
		_ = cardPlay;
		if (base.CombatState == null)
		{
			return;
		}

		await QueenCardCmd.CreateInHand<DelegateLabor>(base.Owner, base.CombatState, isUpgraded: false);
		await QueenCardCmd.CreateInHand<TakeOver>(base.Owner, base.CombatState, isUpgraded: false);
		await QueenCardCmd.AddSoulLamp(choiceContext, base.Owner, 1);
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
