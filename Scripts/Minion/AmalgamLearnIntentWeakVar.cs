using System.Globalization;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>卡面「学习虚弱意图」层数：仅展示基数，与灯槽内 <see cref="AmalgamApplyWeakIntentAction"/> 的 <see cref="AmalgamActionModel.Amount"/> 一致。</summary>
public sealed class AmalgamLearnIntentWeakVar : DynamicVar
{
    public AmalgamLearnIntentWeakVar(decimal baseStacks)
        : base("LearnIntentWeak", baseStacks)
    {
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        _ = card;
        _ = previewMode;
        _ = target;
        _ = runGlobalHooks;
        PreviewValue = BaseValue;
    }

    public override string ToString() => ((int)PreviewValue).ToString(CultureInfo.InvariantCulture);
}

