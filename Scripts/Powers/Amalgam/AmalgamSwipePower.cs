using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace ComicChess.TheQueen;

/// <summary>
/// 顺走（友方聚合体，对齐原版 <c>SWIPE_POWER</c> 图标与命名）：暂存顺走的牌；聚合体逃离战斗时，将每张牌加入战斗奖励。
/// </summary>
public sealed class AmalgamSwipePower : QueenPowerModel, IAmalgamEventListener
{
    private CardModel? _stolenCard;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override bool IsInstanced => true;

    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/swipe_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/swipe_power.png";

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => _stolenCard == null ? [] : [HoverTipFactory.FromCard(_stolenCard)];

    public async Task OnAmalgamEscapeAsync(CombatState combatState, Creature amalgam)
    {
        _ = combatState;
        if (amalgam != base.Owner)
        {
            return;
        }

        if (base.Owner.PetOwner is not Player queen)
        {
            return;
        }

        if (base.CombatState?.RunState.CurrentRoom is not CombatRoom combatRoom)
        {
            return;
        }

        Flash();
        if (_stolenCard != null)
        {
            combatRoom.AddExtraReward(queen, new SpecialCardReward(_stolenCard.DeckVersion ?? _stolenCard, queen));
        }

        await PowerCmd.Remove(this);
    }

    /// <summary>
    /// 按目标敌人的怪物 Id，用 <see cref="MonsterCaptureRewardCatalog"/> 生成对应捕获奖励牌并记入顺走列表（不在牌堆中的新实例直接入表）。
    /// </summary>
    public Task<bool> TryStealMonsterCaptureRewardAsync(Player queen, Creature targetEnemy)
    {
        if (targetEnemy.Monster?.Id.Entry is not { } monsterId)
        {
            return Task.FromResult(false);
        }

        CardModel? card = MonsterCaptureRewardCatalog.TryCreateCaptureRewardCard(queen, monsterId);
        if (card == null)
        {
            return Task.FromResult(false);
        }

        _stolenCard = card;
        return Task.FromResult(true);
    }
}
