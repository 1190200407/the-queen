using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>晨星：魂灯；生成 <see cref="EveningStar"/>；消逝。</summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class MorningStar : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Token;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = false;

	public override int MaxUpgradeLevel => 0;

	protected override IEnumerable<string> RegisteredKeywordIds => [QueenKeyword.Fade];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromCard<EveningStar>(),
		HoverTipFactory.FromPower<SoulLampPower>(),
	];

	public MorningStar()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = choiceContext;
		_ = cardPlay;
		if (base.CombatState is null)
		{
			return;
		}

		await QueenCardCmd.AddSoulLamp(choiceContext, base.Owner, 1);
		await QueenCardCmd.CreateInHand<EveningStar>(base.Owner, base.CombatState, isUpgraded: false);
	}
}
