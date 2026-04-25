using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>镇魂匣：保留；低血目标双倍伤害；斩杀时捕获目标。</summary>
[Pool(typeof(QueenCardPool))]
public sealed class SoulCalmCasket : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    public override bool IsCapture => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
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

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        Creature target = cardPlay.Target;
        bool shouldTriggerFatal = target.Powers.All(static p => p.ShouldOwnerDeathTriggerFatal());

        decimal hpThresholdPercent = base.DynamicVars["HpThresholdPercent"].BaseValue;
        decimal hpThreshold = target.MaxHp * hpThresholdPercent / 100m;
        decimal damage = base.DynamicVars.Damage.BaseValue;
        if (target.CurrentHp <= hpThreshold)
        {
            damage *= 2m;
        }

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
            && attackCommand.Results.Any(static r => r.WasTargetKilled)
            && base.CombatState?.RunState.CurrentRoom is CombatRoom)
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
