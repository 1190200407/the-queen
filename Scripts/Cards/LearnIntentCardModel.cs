using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace ComicChess.TheQueen;

/// <summary>学习意图类卡牌基类：可选先召唤，再按列表依次写入一个或多个意图。</summary>
public abstract class LearnIntentCardModel : QueenCardModel
{
    protected LearnIntentCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    /// <summary>学习意图类卡牌的共通悬浮提示（默认含 Learn Intent）。子类可按需重写。</summary>
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [QueenHoverTips.LearnIntent];

    /// <summary>是否在学习意图前先走召唤流程。</summary>
    protected virtual bool ShouldSummonBeforeLearnIntent => false;

    /// <summary>召唤数值；仅在 <see cref="ShouldSummonBeforeLearnIntent"/> 为 true 时使用。</summary>
    protected virtual decimal GetSummonAmount(PlayerChoiceContext choiceContext, CardPlay cardPlay) => base.DynamicVars.Summon.BaseValue;

    /// <summary>返回本次要学习的意图集合；支持数组/列表与多意图写入。</summary>
    protected abstract Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay);

    protected sealed override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (ShouldSummonBeforeLearnIntent)
        {
            await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, GetSummonAmount(choiceContext, cardPlay), this);
        }

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
}
