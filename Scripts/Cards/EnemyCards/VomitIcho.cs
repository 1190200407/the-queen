using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

/// <summary>呕吐黏液：抽牌并为抽到的牌附魔（黏液）；消耗。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class VomitIcho : QueenCardModel
{
    private const decimal drawCount = 10m;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Draw", drawCount),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..HoverTipFactory.FromEnchantment<Slimed>(),
    ];

    public override int MaxUpgradeLevel => 0;

    public VomitIcho()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        decimal n = base.DynamicVars["Draw"].BaseValue;
        if (n <= 0m)
        {
            return;
        }

        IEnumerable<CardModel> drawn = await CardPileCmd.Draw(choiceContext, n, base.Owner);
        foreach (CardModel card in drawn)
        {
            EnchantmentModel slimed = ModelDb.Enchantment<Slimed>().ToMutable();
            if (!slimed.CanEnchant(card))
            {
                continue;
            }

            CardCmd.Enchant(slimed, card, amount: 1m);
        }
    }
}
