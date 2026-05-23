using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

/// <summary>
/// 学习意图类卡牌基类：可选先召唤，再按列表依次写入一个或多个意图。
/// 默认带 <see cref="QueenCardTags.LearnIntent"/> mod 标签（与 <see cref="ScratchTaggedCard"/> 的 Scratch 标签相同注册方式）。
/// 子类在 <see cref="CreateLearnIntentsAsync"/> 中集中声明本牌对应的意图（组合牌可重写 <see cref="OnPlay"/> 仅用 <see cref="FriendlyAmalgamCmd.CombineIntent"/>）；若出牌顺序需先召唤再插入其它逻辑，可重写
/// <see cref="AfterSummonBeforeLearnIntentsAsync"/>；若完全自定义出牌流程，可重写 <see cref="OnPlay"/> 并在适当时机调用
/// <see cref="PlayLearnIntentsFromCreateAsync"/>。
/// </summary>
public abstract class LearnIntentCardModel : QueenCardModel
{
    protected LearnIntentCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override IEnumerable<string> RegisteredCardTagIds => [QueenCardTags.LearnIntent];

    /// <summary>友方聚合体三灯槽均已有意图时金闪（打出后当场执行一次再学；与是否能力沉睡无关，仅死亡不金闪）。</summary>
    protected override bool ShouldGlowGoldInternal =>
        (base.Owner?.Creature?.CombatState is { } combatState
            && FriendlyAmalgamCmd.GetExisting(combatState, base.Owner) is { Monster: FriendlyAmalgam amalgam }
            && amalgam.Creature.IsAlive
            && amalgam.HasAllTorchSlotsFilled
            && !amalgam.BlockActionFromSleep);

    /// <summary>学习意图类卡牌的共通悬浮提示（默认含 Learn Intent）。子类可按需重写。</summary>
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [QueenHoverTips.LearnIntent];

    /// <summary>是否在学习意图前先走召唤流程。</summary>
    protected virtual bool ShouldSummonBeforeLearnIntent => false;

    /// <summary>召唤数值；仅在 <see cref="ShouldSummonBeforeLearnIntent"/> 为 true 时使用。</summary>
    protected virtual decimal GetSummonAmount(PlayerChoiceContext choiceContext, CardPlay cardPlay) => base.DynamicVars.Summon.BaseValue;

    /// <summary>
    /// 在可选召唤之后、执行 <see cref="CreateLearnIntentsAsync"/> 与学习写入之前调用（例如上能力、塞 token 牌）。
    /// </summary>
    protected virtual Task AfterSummonBeforeLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Task.CompletedTask;

    /// <summary>
    /// 返回本次要学习的意图集合；由 <see cref="PlayLearnIntentsFromCreateAsync"/> 逐个 <see cref="FriendlyAmalgamCmd.LearnIntent"/> 写入。
    /// 组合意图牌（<see cref="QueenKeyword.AmalgamComposite"/>）走 <see cref="FriendlyAmalgamCmd.CombineIntent"/>，可重写 <see cref="OnPlay"/> 且不调用本方法，则保留默认空实现即可。
    /// </summary>
    protected virtual Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([]);

    /// <summary>
    /// 调用 <see cref="CreateLearnIntentsAsync"/> 并对每个非空意图执行 <see cref="FriendlyAmalgamCmd.LearnIntent"/>。
    /// 完全自定义 <see cref="OnPlay"/> 时请在合适时机调用本方法以复用学习逻辑。
    /// </summary>
    protected async Task PlayLearnIntentsFromCreateAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IReadOnlyList<AmalgamActionModel?> intents = await CreateLearnIntentsAsync(choiceContext, cardPlay);
        foreach (AmalgamActionModel? intent in intents)
        {
            if (intent == null)
            {
                continue;
            }

            await FriendlyAmalgamCmd.LearnIntent(choiceContext, base.Owner, intent, this);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (ShouldSummonBeforeLearnIntent)
        {
            await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, GetSummonAmount(choiceContext, cardPlay), this);
        }

        await AfterSummonBeforeLearnIntentsAsync(choiceContext, cardPlay);
        await PlayLearnIntentsFromCreateAsync(choiceContext, cardPlay);
    }
}
