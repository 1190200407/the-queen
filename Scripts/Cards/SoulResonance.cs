using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Cards.DynamicVars;

namespace ComicChess.TheQueen;

/// <summary>灵魂同调：为所有玩家召唤聚合体；持有本能力时，你打出学习意图类牌会使所有玩家的聚合体学习相同意图（见 <see cref="FriendlyAmalgamCmd.LearnIntent"/> 末尾同步）。</summary>

public sealed class SoulResonance : QueenCardModel
{
	private const decimal summonBase = 10m;
	private const int energyCost = 3;
	private const CardType type = CardType.Power;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new SummonVar(summonBase).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
	];

	public SoulResonance()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = cardPlay;
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

		CombatState? combatState = base.Owner.Creature.CombatState;
		if (combatState != null)
		{
			decimal summonAmount = base.DynamicVars.Summon.BaseValue;
			foreach (Player player in combatState.Players)
			{
				if (!player.Creature.IsAlive)
				{
					continue;
				}

				await FriendlyAmalgamCmd.Summon(choiceContext, player, summonAmount, this);
			}
		}
		await PowerCmd.Apply<SoulResonancePower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Summon.UpgradeValueBy(3m);
	}
}
