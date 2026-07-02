using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;


[RegisterCard(typeof(QueenCardPool))]
public sealed class StrongAdaptability : QueenCardModel
{
	private const int energyCost = 2;
	private const CardType type = CardType.Power;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [.. HoverTipFactory.FromAffliction<Bound>()];

	internal override bool HasSelfBound => true;

	public StrongAdaptability()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.IsUpgraded)
		{
			CardModel? picked = await CardSelectCmd.FromHandForUpgrade(choiceContext, base.Owner, this);
			if (picked != null)
			{
				CardCmd.Upgrade(picked);
			}
		}

<<<<<<< HEAD
		await PowerCmd.Apply<StrongAdaptabilityPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
=======
		await PowerCmd.Apply<StrongAdaptabilityPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
>>>>>>> beta
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
	}
}
