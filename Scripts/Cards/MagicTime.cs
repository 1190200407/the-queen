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
using MegaCrit.Sts2.Core.Localization.DynamicVars;
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

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("SoulLampOnPlay", 1m)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		base.EnergyHoverTip,
		QueenHoverTips.SoulLamp,
		.. HoverTipFactory.FromAffliction<Bound>(),
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

		int lampGain = (int)base.DynamicVars["SoulLampOnPlay"].BaseValue;
		if (lampGain > 0)
		{
			await QueenCardCmd.AddSoulLamp(player, lampGain);
		}

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
		base.DynamicVars["SoulLampOnPlay"].UpgradeValueBy(1m);
	}
}
