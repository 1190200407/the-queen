using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace ComicChess.TheQueen;

/// <summary>掠食同化：聚合体以斩杀击杀普通/精英敌怪时，按图鉴给予捕获奖励。</summary>
public sealed class PredatoryAssimilationPower : QueenPowerModel, IAmalgamEventListener
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

    private sealed class Data
    {
        public bool IsUpgraded = false;
    }

    protected override object? InitInternalData() => new Data();

    internal bool IsUpgraded => GetInternalData<Data>()?.IsUpgraded ?? false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Upgraded", 0)];

    internal void ConfigureIsUpgraded(bool isUpgraded, bool flash = false)
    {
        GetInternalData<Data>().IsUpgraded = isUpgraded;
        base.DynamicVars["Upgraded"].BaseValue = isUpgraded ? 1 : 0;
		if (flash)
		{
			Flash();
		}
    }

	public async Task OnAmalgamDamagedCreatureAsync(CombatState combatState, PlayerChoiceContext choiceContext, Creature amalgam, Creature damagedEnemy, IEnumerable<DamageResult> damageResults)
	{
		if (amalgam.PetOwner is not Player queen)
		{
			return;
		}

		if (QueenDamageResults.AnyTargetKilled(damageResults))
		{
            await TryGrantCaptureAfterAmalgamFatalKillAsync(choiceContext, queen, damagedEnemy);
		}
	}

	/// <summary>
	/// 由 <see cref="FriendlyAmalgamHook.AfterAmalgamDamagedCreature"/> 在聚合体造成伤害且
	/// <see cref="DamageResult.WasTargetKilled"/> 为真时调用。
	/// </summary>
	private async Task TryGrantCaptureAfterAmalgamFatalKillAsync(
		PlayerChoiceContext choiceContext,
		Player queen,
		Creature victim)
	{
		_ = choiceContext;
		if (queen.Creature.GetPower<PredatoryAssimilationPower>() is null)
		{
			return;
		}

		if (victim is not { IsEnemy: true })
		{
			return;
		}

		RoomType? roomType = MonsterCaptureRewardCatalog.GetEncounterRoomType(base.Owner.CombatState);
		bool check = roomType is RoomType.Monster || (roomType is RoomType.Elite && IsUpgraded);
		if (!check)
		{
			return;
		}

		if (!victim.Powers.All(static p => p.ShouldOwnerDeathTriggerFatal()))
		{
			return;
		}

		CombatRoom? combatRoom = queen.RunState?.CurrentRoom as CombatRoom;
		if (combatRoom is null)
		{
			return;
		}

		CardModel? reward = MonsterCaptureRewardCatalog.TryCreateCaptureRewardCard(queen, victim);
		if (reward is null)
		{
			return;
		}

		combatRoom.AddExtraReward(queen, new SpecialCardReward(reward, queen));
		await CaptureSuccessPower.ApplyForCapture(queen, reward, captureSourceCard: null);
	}
}
