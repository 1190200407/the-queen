using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Cards.DynamicVars;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;


[RegisterCard(typeof(QueenCardPool))]
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
		new SummonVar(nextTurnSummon).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC")
	];

	public EmergencyEvasion()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = cardPlay;
		ICombatState? combatState = base.Owner.Creature.CombatState;
		if (combatState == null)
		{
			return;
		}

		Creature? amalgamCreature = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
		if (amalgamCreature?.Monster is FriendlyAmalgam)
		{
			await PowerCmd.Apply<AmalgamSleepPower>(choiceContext, amalgamCreature, 1m, base.Owner.Creature, this);
		}

		await PowerCmd.Apply<NextTurnAmalgamSummonPendingPower>(choiceContext,
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
