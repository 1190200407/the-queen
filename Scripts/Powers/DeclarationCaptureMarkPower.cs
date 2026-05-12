using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace ComicChess.TheQueen;

/// <summary>宣告标记（可捕获）：仅由 <see cref="Declaration"/> 在 <see cref="ICanMonsterCapture.CanCapture"/> 为真时施加；持有者死亡时由施加者结算捕获。</summary>
public sealed class DeclarationCaptureMarkPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool IsInstanced => true;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		_ = choiceContext;
		_ = player;
		await PowerCmd.Decrement(this);
		if (Amount <= 0m)
		{
			await PowerCmd.Remove(this);
		}
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		_ = choiceContext;
		_ = deathAnimLength;
		if (creature != base.Owner || wasRemovalPrevented || Amount <= 0m)
		{
			return;
		}

		Player? capturer = base.Applier?.Player;
		CombatRoom? combatRoom = capturer?.RunState?.CurrentRoom as CombatRoom;
		if (capturer == null || combatRoom == null)
		{
			return;
		}

		bool shouldTriggerFatal = creature.Powers.All(static p => p.ShouldOwnerDeathTriggerFatal());
		if (!shouldTriggerFatal)
		{
			return;
		}

		CardModel? reward = MonsterCaptureRewardCatalog.TryCreateCaptureRewardCard(capturer, creature);
		if (reward is { } rewardCard)
		{
			combatRoom.AddExtraReward(capturer, new SpecialCardReward(rewardCard, capturer));
			CaptureSuccessPower? applied = await PowerCmd.Apply<CaptureSuccessPower>(
				capturer.Creature,
				1m,
				capturer.Creature,
				null);
			if (applied != null)
			{
				applied.RewardCard = rewardCard;
			}
		}

		await PowerCmd.Remove(this);
	}
}
