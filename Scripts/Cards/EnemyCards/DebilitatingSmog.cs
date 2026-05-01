using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>衰弱烟雾：降低目标本回合力量，你获得力量；魂缚、消耗。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class DebilitatingSmog : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    private const decimal enemyStrengthLoss = 2m;
    private const decimal playerStrengthGain = 2m;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override int MaxUpgradeLevel => 0;

    internal override bool HasSelfBound => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("EnemyStrengthLoss", enemyStrengthLoss),
        new IntVar("PlayerStrengthGain", playerStrengthGain),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..HoverTipFactory.FromAffliction<Bound>(),
        HoverTipFactory.FromPower<AmalgamIntentStrengthDownPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    public DebilitatingSmog()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        Creature target = cardPlay.Target;
        if (!target.IsAlive)
        {
            return;
        }

        decimal loss = base.DynamicVars["EnemyStrengthLoss"].BaseValue;
        decimal grant = base.DynamicVars["PlayerStrengthGain"].BaseValue;

        if (loss > 0m)
        {
            await PowerCmd.Apply<AmalgamIntentStrengthDownPower>(target, loss, base.Owner.Creature, this);
        }

        if (grant > 0m && base.Owner.Creature.IsAlive)
        {
            await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, grant, base.Owner.Creature, this);
        }
    }
}
