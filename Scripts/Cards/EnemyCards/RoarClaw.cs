using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>咆哮爪击：召唤；聚合体先施加易伤，再学习 2 连击进攻意图。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class RoarClaw : LearnIntentCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    private const int learnIntentRepeat = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(10m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new AmalgamLearnIntentVulnerableVar(3m),
        new AmalgamLearnIntentDamageVar(5m, ValueProp.Move),
    ];
    public override int MaxUpgradeLevel => 0;

    protected override bool ShouldSummonBeforeLearnIntent => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        QueenHoverTips.LearnIntent,
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    /// <summary>无友方聚合体或 <see cref="FriendlyAmalgam.BlockActionFromSleep"/> 时手牌红高亮（打出时先有聚合体对敌易伤再学意图）。</summary>
    protected override bool ShouldGlowRedInternal =>
        (base.Owner?.Creature?.CombatState is { } combatState
            && (FriendlyAmalgamCmd.GetExisting(combatState, base.Owner) is not { Monster: FriendlyAmalgam amalgam }
                || amalgam.sleepReason.HasFlag(FriendlyAmalgam.SleepReason.Power)))
        || base.ShouldGlowRedInternal;

    public RoarClaw()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task AfterSummonBeforeLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (base.Owner.Creature is not { IsAlive: true })
        {
            return;
        }

        ICombatState? combatState = base.Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        if (FriendlyAmalgamCmd.GetExisting(combatState, base.Owner) is { Monster: FriendlyAmalgam fam, IsAlive: true } amalgamCreature
            && !fam.BlockActionFromSleep)
        {
            decimal stacks = base.DynamicVars["LearnIntentVulnerable"].BaseValue;
            Creature? selectedEnemy = cardPlay.Target is { IsAlive: true } t ? t : null;
            await AmalgamActionRegistry.ExecuteTemporaryAsync(
                choiceContext,
                amalgamCreature,
                AmalgamActionRegistry.CreateVulnerable(stacks, selectedEnemy));
        }
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        decimal dmg = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateOffenseMulti(dmg, learnIntentRepeat);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}
