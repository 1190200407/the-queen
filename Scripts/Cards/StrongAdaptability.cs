using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class StrongAdaptability : QueenCardModel
{
	private const int energyCost = 2;
	private const CardType type = CardType.Power;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [.. HoverTipFactory.FromAffliction<Bound>()];

	internal override bool UseBoundAfflictionOverlayForPreview => true;

	public StrongAdaptability()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	public override async Task BeforeCombatStart()
	{
		if (base.Owner?.Creature?.CombatState == null)
		{
			return;
		}
		if (Affliction is not Bound)
		{
			await CardCmd.Afflict<Bound>(this, 1m);
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<StrongAdaptabilityPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
	}
}
