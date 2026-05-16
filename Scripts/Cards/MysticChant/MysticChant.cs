using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;


[RegisterCard(typeof(QueenCardPool))]
public sealed class MysticChant : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
		HoverTipFactory.FromCard<FirstChant>(upgrade: false),
		HoverTipFactory.FromCard<SecondChant>(upgrade: false),
		HoverTipFactory.FromCard<FinalChant>(upgrade: false),
		.. HoverTipFactory.FromAffliction<Bound>(),
		HoverTipFactory.FromPower<SoulLampPower>()
	];

	public MysticChant()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.CombatState == null)
		{
			return;
		}

		await QueenCardCmd.CreateInHand<FirstChant>(base.Owner, base.CombatState, isUpgraded: false);
		await QueenCardCmd.CreateInHand<SecondChant>(base.Owner, base.CombatState, isUpgraded: false);
		await QueenCardCmd.CreateInHand<FinalChant>(base.Owner, base.CombatState, isUpgraded: false);

		if (base.IsUpgraded)
		{
			await QueenCardCmd.AddSoulLamp(base.Owner);
		}
	}
}
