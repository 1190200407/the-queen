using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;
public sealed class Dazed : QueenEnchantmentModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];

    protected override void OnEnchant()
    {
        Card.AddKeyword(CardKeyword.Ethereal);
    }
}