using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 按怪物 <see cref="AbstractId.Entry"/> 决定「捕获」成功时的额外卡牌奖励类型。
/// 未配置的怪物返回 <c>null</c>：不展示意图位奖励预览，也不在捕获成功时给予该奖励。
/// </summary>
public static class MonsterCaptureRewardCatalog
{
    /// <summary>原版 <see cref="MegaCrit.Sts2.Core.Models.Monsters.Flyconid"/> 的 Id。</summary>
    public const string Flyconid = "FLYCONID";

    private static readonly Dictionary<string, Func<Player, CardModel>> RewardCreators =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { Flyconid, static owner => owner.RunState!.CreateCard<FrailSpores>(owner) },
        };

    /// <summary>为捕获预览或 <see cref="CaptureSuccessPower"/> 创建奖励牌实例；无配置时返回 <c>null</c>。</summary>
    public static CardModel? TryCreateCaptureRewardCard(Player owner, Creature enemy)
    {
        if (enemy.Monster?.Id.Entry is not { } monsterId
            || owner.RunState is null
            || !RewardCreators.TryGetValue(monsterId, out Func<Player, CardModel>? create))
        {
            return null;
        }

        return create(owner);
    }
}
