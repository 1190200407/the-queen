using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace ComicChess.TheQueen;

/// <summary>
/// 「捕获」奖励在意图位预览时的统一规则：仅当 <see cref="QueenHoverTips.CardHasCaptureHoverTip"/> 已成立时调用。
/// 与 <see cref="MonsterCaptureRewardCatalog"/> 及 <see cref="CaptureSuccessPower"/> 结算条件对齐（预览不要求本次伤害已击杀）。
/// </summary>
internal static class CaptureRewardPreviewRules
{
    internal static CardModel? TryCreatePreviewCard(Player owner, Creature target)
    {
        if (!target.IsEnemy
            || target.CombatState?.Encounter?.RoomType == RoomType.Boss
            || owner.RunState?.CurrentRoom is not CombatRoom
            || !target.Powers.All(static p => p.ShouldOwnerDeathTriggerFatal()))
        {
            return null;
        }

        return MonsterCaptureRewardCatalog.TryCreateCaptureRewardCard(owner, target);
    }
}
