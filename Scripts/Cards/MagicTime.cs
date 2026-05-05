using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class MagicTime : QueenCardModel
{
	private const int energyCost = 3;
	private const CardType type = CardType.Power;
	private const CardRarity rarity = CardRarity.Ancient;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		base.EnergyHoverTip,
		HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
		.. HoverTipFactory.FromAffliction<Bound>()
	];

	public MagicTime()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = choiceContext;
		_ = cardPlay;
		Player player = base.Owner;
		ArgumentNullException.ThrowIfNull(player.PlayerCombatState, nameof(player.PlayerCombatState));

		foreach (CardModel card in player.PlayerCombatState.AllCards.ToList())
		{
			CardCmd.ClearAffliction(card);
			await CardCmd.Afflict<Bound>(card, 1m);
		}

		await PowerCmd.Apply<MagicTimePower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
	}

    protected override void OnUpgrade()
    {
		RemoveKeyword(CardKeyword.Ethereal);
    }
}
