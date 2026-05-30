using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace ComicChess.TheQueen;

/// <summary>生态箱标记：持有者在可捕获场合死亡时，由施加者获得与宣告相同的额外捕获卡奖励；施加者取自 <see cref="PowerModel.Applier"/>。精英战也可触发。</summary>
public sealed class EcosphereCaptureMarkPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [QueenHoverTips.Capture];

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		_ = choiceContext;
		_ = deathAnimLength;
		if (creature != base.Owner || wasRemovalPrevented)
		{
			return;
		}

		Player? applier = base.Applier?.Player;
		if (applier == null)
		{
			return;
		}

		if (!creature.Powers.All(static p => p.ShouldOwnerDeathTriggerFatal()))
		{
			return;
		}

		CombatRoom? combatRoom = applier.RunState?.CurrentRoom as CombatRoom;
		if (combatRoom == null)
		{
			return;
		}

		CardModel? rewardCard = MonsterCaptureRewardCatalog.TryCreateCaptureRewardCard(applier, creature);
		if (rewardCard is null)
		{
			return;
		}

		combatRoom.AddExtraReward(applier, new SpecialCardReward(rewardCard, applier));
		await CaptureSuccessPower.ApplyForCapture(applier, rewardCard, captureSourceCard: null);
		await PowerCmd.Remove(this);
	}
}
