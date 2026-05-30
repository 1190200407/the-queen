using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>镇魂匣：保留；低血目标双倍伤害；斩杀时捕获目标�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class SoulCalmCasket : QueenCardModel, ICanMonsterCapture
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public bool CanCapture(MonsterModel monster, CombatState combatState) =>
        monster is not null && combatState is not null && IsMutable;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Fatal),
        QueenHoverTips.Capture,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(15m, ValueProp.Move), new IntVar("HpThresholdPercent", 10m)];

    protected override bool ShouldGlowGoldInternal => base.CombatState?.HittableEnemies.Any((Creature e) => e.CurrentHp <= e.MaxHp * base.DynamicVars["HpThresholdPercent"].BaseValue / 100m) ?? false;

    public SoulCalmCasket()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer != base.Owner.Creature)
            return 1m;
        if (target == null)
            return 1m;
        if (target.CurrentHp <= target.MaxHp * base.DynamicVars["HpThresholdPercent"].BaseValue / 100m)
            return 2m;
        return 1m;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        Creature target = cardPlay.Target;
        decimal damage = base.DynamicVars.Damage.BaseValue;
        bool shouldTriggerFatal = target.Powers.All(static p => p.ShouldOwnerDeathTriggerFatal());

        AttackCommand attackCommand = await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        CombatRoom? combatRoom = base.CombatState?.RunState.CurrentRoom as CombatRoom;
        if (combatRoom is null)
        {
            return;
        }
        
        if (shouldTriggerFatal
            && QueenDamageResults.AnyTargetKilled(attackCommand))
        {
            CardModel? reward = MonsterCaptureRewardCatalog.TryCreateCaptureRewardCard(base.Owner, target);
            if (reward is { } rewardCard)
            {
                combatRoom.AddExtraReward(base.Owner, new SpecialCardReward(rewardCard, base.Owner));
                await CaptureSuccessPower.ApplyForCapture(base.Owner, rewardCard, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(5m);
        base.EnergyCost.UpgradeBy(-1);
        base.DynamicVars["HpThresholdPercent"].UpgradeValueBy(5m);
    }
}
