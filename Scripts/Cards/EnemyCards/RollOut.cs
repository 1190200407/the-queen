using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>翻滚：召唤；聚合体沉睡；两次 <see cref="FriendlyAmalgamCmd.CombineIntent"/> 学习伤害与力量。消耗。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class RollOut : QueenCardModel
{
    public const string SlumberingBeetleCompositeKey = "BOWLBUG";

    private const decimal summon = 7m;
    private const decimal learnIntentDamage = 16m;
    private const decimal learnIntentStrength = 2m;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, QueenKeyword.amalgamComposite];

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(summon).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentDamageVar(learnIntentDamage, ValueProp.Move),
        new AmalgamLearnIntentStrengthVar(learnIntentStrength),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(QueenKeyword.amalgamComposite),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    public RollOut()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);

        if (base.Owner.Creature.CombatState is not { } combatState)
        {
            return;
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is { IsAlive: true })
        {
            await PowerCmd.Apply<AmalgamSleepPower>(amalgam, 1m, base.Owner.Creature, this);
        }

        decimal damage = base.DynamicVars["LearnIntentDamage"].BaseValue;
        decimal strength = base.DynamicVars["LearnIntentStrength"].BaseValue;

        await FriendlyAmalgamCmd.CombineIntent(
            choiceContext,
            base.Owner,
            new AmalgamOffenseIntentAction(damage),
            this,
            SlumberingBeetleCompositeKey);

        await FriendlyAmalgamCmd.CombineIntent(
            choiceContext,
            base.Owner,
            new AmalgamGainStrengthIntentAction(strength),
            this,
            SlumberingBeetleCompositeKey);
    }
}
