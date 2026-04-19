using System.Globalization;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 卡面「学习格挡意图」数字：仅展示基数 + 附魔；不在此预走 <c>Hook.ModifyBlock</c>，避免卡面与 <see cref="MegaCrit.Sts2.Core.Commands.CreatureCmd.GainBlock"/> 叠敏捷。
/// </summary>
public sealed class AmalgamLearnIntentBlockVar : DynamicVar
{
	public AmalgamLearnIntentBlockVar(decimal baseBlock)
		: base("LearnIntentBlock", baseBlock)
	{
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		//暂时没有能力能给这个数加数值，所以直接显示基础值
		decimal num = BaseValue;
		PreviewValue = num;
	}

	public override string ToString() => ((int)PreviewValue).ToString(CultureInfo.InvariantCulture);
}
