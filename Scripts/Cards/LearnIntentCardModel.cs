using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace ComicChess.TheQueen;

/// <summary>
/// 学习意图类卡牌基类：可选先召唤，再按列表依次写入一个或多个意图。
/// 默认带 <see cref="QueenCardTags.LearnIntent"/>（与 <see cref="ScratchTaggedCard"/> / <see cref="QueenCardTags.Scratch"/> 相同做法）。
/// 子类在 <see cref="CreateLearnIntentsAsync"/> 中集中声明本牌对应的意图，便于查阅；若出牌顺序需先召唤再插入其它逻辑，可重写
/// <see cref="AfterSummonBeforeLearnIntentsAsync"/>；若完全自定义出牌流程，可重写 <see cref="OnPlay"/> 并在适当时机调用
/// <see cref="PlayLearnIntentsFromCreateAsync"/>。
/// </summary>
public abstract class LearnIntentCardModel : QueenCardModel
{
    protected LearnIntentCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    /// <summary>除 <see cref="QueenCardTags.LearnIntent"/> 外附加的 <see cref="CardTag"/>。</summary>
    protected virtual IEnumerable<CardTag> AdditionalCanonicalTags() => [];

    protected override HashSet<CardTag> CanonicalTags
    {
        get
        {
            HashSet<CardTag> h = new();
            foreach (CardTag t in AdditionalCanonicalTags())
            {
                h.Add(t);
            }

            h.Add(QueenCardTags.LearnIntent);
            return h;
        }
    }

    /// <summary>学习意图类卡牌的共通悬浮提示（默认含 Learn Intent）。子类可按需重写。</summary>
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [QueenHoverTips.LearnIntent];

    /// <summary>是否在学习意图前先走召唤流程。</summary>
    protected virtual bool ShouldSummonBeforeLearnIntent => false;

    /// <summary>召唤数值；仅在 <see cref="ShouldSummonBeforeLearnIntent"/> 为 true 时使用。</summary>
    protected virtual decimal GetSummonAmount(PlayerChoiceContext choiceContext, CardPlay cardPlay) => base.DynamicVars.Summon.BaseValue;

    /// <summary>
    /// 在可选召唤之后、执行 <see cref="CreateLearnIntentsAsync"/> 与学习写入之前调用（例如上能力、塞 token 牌）。
    /// </summary>
    protected virtual Task AfterSummonBeforeLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Task.CompletedTask;

    /// <summary>返回本次要学习的意图集合；支持数组/列表与多意图写入。</summary>
    protected abstract Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay);

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
