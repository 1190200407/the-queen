using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class GemCharge : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Power;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromCard<PiercingCharge>(upgrade: base.IsUpgraded),
		HoverTipFactory.FromCard<DiffuseCharge>(upgrade: base.IsUpgraded),
		.. HoverTipFactory.FromAffliction<Bound>(),
		QueenHoverTips.SoulLamp
	];

	public GemCharge()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.CombatState == null)
		{
			return;
		}

		// 只有本体升级后，才生成升级版衍生牌。
		await QueenCardCmd.CreateInHand<PiercingCharge>(base.Owner, base.CombatState, isUpgraded: base.IsUpgraded);
		await QueenCardCmd.CreateInHand<DiffuseCharge>(base.Owner, base.CombatState, isUpgraded: base.IsUpgraded);
		await QueenCardCmd.AddSoulLamp(base.Owner);
	}
}
