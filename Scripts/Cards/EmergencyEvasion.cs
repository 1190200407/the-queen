using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class EmergencyEvasion : QueenCardModel
{
	private const decimal nextTurnSummon = 7m;
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Common;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new SummonVar(nextTurnSummon).WithTooltip("QUEEN_SUMMON_DYNAMIC")
	];

	public EmergencyEvasion()
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
		if (amalgamCreature is { IsAlive: true })
		{
            // 进入“能力导致沉睡”：持续到下回合开始（由 AmalgamSleepPower 自行倒计时并苏醒）。
			await PowerCmd.Apply<AmalgamSleepPower>(amalgamCreature, 1m, base.Owner.Creature, this);
		}
		
		await PowerCmd.Apply<EmergencyEvasionPendingPower>(
			base.Owner.Creature,
			base.DynamicVars.Summon.BaseValue,
			base.Owner.Creature,
			this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Summon.UpgradeValueBy(3m);
	}
}
