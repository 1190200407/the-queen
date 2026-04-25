using System.Globalization;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>卡面「学习力量意图」层数：仅展示基数 + 附魔，与灯槽内 <see cref="AmalgamGainStrengthIntentAction"/> 的 <see cref="AmalgamActionModel.Amount"/> 一致。</summary>
public sealed class AmalgamLearnIntentStrengthVar : DynamicVar
{
    public AmalgamLearnIntentStrengthVar(decimal baseStrength)
        : base("LearnIntentStrength", baseStrength)
    {
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        decimal num = BaseValue;
        PreviewValue = num;
    }

    public override string ToString() => ((int)PreviewValue).ToString(CultureInfo.InvariantCulture);
}
