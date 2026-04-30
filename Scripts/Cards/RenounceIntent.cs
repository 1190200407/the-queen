using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class RenounceIntent : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Common;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [QueenHoverTips.ForgetIntent];

	public RenounceIntent()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = cardPlay;
		CombatState? combatState = base.Owner.Creature.CombatState;
		if (combatState == null)
		{
			return;
		}

		Creature? amalgamCreature = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
		if (amalgamCreature?.Monster is not FriendlyAmalgam amalgam || !amalgamCreature.IsAlive)
		{
			return;
		}

		await amalgam.ActCurrentIntentImmediatelyAsync(choiceContext);
		await amalgam.ForgetCurrentTorchSlotIntentAsync();
	}

    protected override void OnUpgrade()
    {
		RemoveKeyword(CardKeyword.Exhaust);
    }
}
