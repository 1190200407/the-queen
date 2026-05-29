using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

/// <summary>曼陀罗：为 1 张技能或攻击牌添加重放与消逝；消耗（升级后不再消耗）。</summary>
[RegisterCard(typeof(QueenCardPool))]
public sealed class Mandragora : QueenCardModel
{
    private static readonly CardKeyword FadeKeyword = ModKeywordRegistry.GetCardKeyword(QueenKeyword.Fade);

    private const int replayGain = 1;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic),
        ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade),
    ];

    public Mandragora()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (base.Owner == null)
        {
            return;
        }

        IEnumerable<CardModel> selected = await CardSelectCmd.FromHand(
            choiceContext,
            base.Owner,
            new CardSelectorPrefs(new LocString("cards", "STS2_COMICCHESS_THEQUEEN_CARD_MANDRAGORA.selectionPrompt"), 1),
            CanEnchant,
            this);

        CardModel? card = selected.FirstOrDefault();
        if (card == null)
        {
            return;
        }

        ApplyEnchantments(card);
        CardCmd.Preview(card);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }

    private static bool CanEnchant(CardModel card) =>
        card.Type is CardType.Attack or CardType.Skill;

    private static void ApplyEnchantments(CardModel card)
    {
        if (!card.HasModKeyword(FadeKeyword))
        {
            card.AddModKeyword(FadeKeyword);
        }

        card.BaseReplayCount += replayGain;
    }
}
