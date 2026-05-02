using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

/// <summary>翱翔：学习意图为获得翱翔；消耗。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class Soar : QueenCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private const decimal summon = 5m;
    private const decimal soarStacks = 1m;
    private const string soarEntryId = "COMICCHESS-AMALGAM_SOAR_POWER";

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(summon).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new PowerVar<AmalgamSoarPower>(soarStacks),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        QueenHoverTips.LearnIntent,
        HoverTipFactory.FromPower<AmalgamSoarPower>(),
    ];

    public Soar()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);

        decimal stacks = base.DynamicVars.Power<AmalgamSoarPower>().BaseValue;
        if (stacks <= 0m)
        {
            return;
        }

        await FriendlyAmalgamCmd.LearnIntent(
            choiceContext,
            base.Owner,
            new AmalgamGainBuffIntentAction<AmalgamSoarPower>(stacks, soarEntryId),
            this);
    }
}

