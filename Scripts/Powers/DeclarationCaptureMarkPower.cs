using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace ComicChess.TheQueen;

/// <summary>宣告标记：2 回合内死亡则由施加者捕获（仅普通/精英敌怪）。</summary>
public sealed class DeclarationCaptureMarkPower : QueenPowerModel
{
    private sealed class Data
    {
        public Player? Capturer;
        public bool isUpgraded;
    }

    protected override object? InitInternalData() => new Data();

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool IsInstanced => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("Upgraded", 0),
    ];

    internal void ConfigureCapturer(Player capturer, bool isUpgraded)
    {
        GetInternalData<Data>().Capturer = capturer;
        GetInternalData<Data>().isUpgraded = isUpgraded;
        base.DynamicVars["Upgraded"].BaseValue = isUpgraded ? 1 : 0;
    }

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

        RoomType? roomType = creature.CombatState?.Encounter?.RoomType;
        if (roomType is not (RoomType.Monster or RoomType.Event or RoomType.Elite))
        {
            return;
        }
        if (roomType is RoomType.Elite && !GetInternalData<Data>().isUpgraded)
        {
            return;
        }

        Player? capturer = GetInternalData<Data>().Capturer;
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
