using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>烟雾弥漫：SmoggyPower + Smog（原版）；被侵蚀的牌获得重放2次（�?mod）�?/summary>
[Pool(typeof(EnemyCardPool))]
public sealed class Smoggy : LearnIntentCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        HoverTipFactory.FromPower<SmoggyPower>(),
        ..HoverTipFactory.FromAffliction<Smog>(),
    ];

    public override int MaxUpgradeLevel => 0;

    public Smoggy()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        AmalgamActionModel intent = new AmalgamSpecialIntentAction(
            moveId: "AMALGAM_INTENT_SPECIAL_SMOGGY",
            intentDescriptionKey: "AMALGAM_SPECIAL_SMOGGY.description",
            execute: async (PlayerChoiceContext _, Creature amalgam, Creature owner) =>
            {
                await CreatureCmd.TriggerAnim(amalgam, "Cast", AmalgamSpecialIntentAction.CastAnimDelay);
                await PowerCmd.Apply<SmoggyPower>(owner, 1m, applier: amalgam, cardSource: null);
                await PowerCmd.Apply<BurstPower>(owner, 2m, applier: amalgam, cardSource: null);
            });

        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}

