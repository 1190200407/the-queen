using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>缩小射线：聚合体使所有敌人缩小；消耗。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class Shrinker : QueenCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    private const decimal shrinkStacks = 3m;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ShrinkPower>(shrinkStacks),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<ShrinkPower>(),
    ];

    /// <summary>无友方聚合体或沉睡时红高亮（打出时由聚合体直接施放缩小射线）。</summary>
    protected override bool ShouldGlowRedInternal =>
        (base.Owner?.Creature?.CombatState is { } combatState
            && (FriendlyAmalgamCmd.GetExisting(combatState, base.Owner) is not { Monster: FriendlyAmalgam amalgam }
                || amalgam.BlockActionFromSleep))
        || base.ShouldGlowRedInternal;

    public Shrinker()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (base.Owner.Creature.CombatState is not { } combatState)
        {
            return;
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is not { IsAlive: true }
            || amalgam.Monster is not FriendlyAmalgam amalgamModel
            || amalgamModel.BlockActionFromSleep)
        {
            return;
        }

        decimal stacks = base.DynamicVars.Power<ShrinkPower>().BaseValue;
        if (stacks <= 0m)
        {
            return;
        }

        AmalgamActionModel? shrinkRay = AmalgamActionRegistry.CreateShrinkRay(stacks);
        if (shrinkRay != null)
        {
            await shrinkRay.ExecuteAsync(choiceContext, amalgam);
        }
    }
}
