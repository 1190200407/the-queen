using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>处决：基础伤害 + 目标负面加成；斩杀时捕获目标�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class Execution : QueenCardModel, ICanMonsterCapture
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public bool CanCapture(MonsterModel monster, CombatState combatState) =>
        monster is not null && combatState is not null;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(10m),
        new ExtraDamageVar(4m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (CardModel card, Creature? target) =>
            target?.Powers.Count(static p => p.Type == PowerType.Debuff) ?? 0),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Fatal),
        QueenHoverTips.Capture,
    ];

    public Execution()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        Creature target = cardPlay.Target;
        bool shouldTriggerFatal = target.Powers.All(static p => p.ShouldOwnerDeathTriggerFatal());

        AttackCommand attackCommand = await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        CombatRoom? combatRoom = base.CombatState?.RunState.CurrentRoom as CombatRoom;
        if (combatRoom is null)
        {
            return;
        }
        Log.Info($"Execution: shouldTriggerFatal: {shouldTriggerFatal}, attackCommand.Results: {attackCommand.Results.Count(static r => r.WasTargetKilled)}");

        if (shouldTriggerFatal
            && attackCommand.Results.Any(static r => r.WasTargetKilled)
            && base.CombatState?.RunState.CurrentRoom is CombatRoom)
        {
            Log.Info($"Execution: Capturing target: {target.Name}");
            CardModel? reward = MonsterCaptureRewardCatalog.TryCreateCaptureRewardCard(base.Owner, target);
            Log.Info($"Execution: Reward: {reward?.Title}");
            if (reward is { } rewardCard)
            {
                combatRoom.AddExtraReward(base.Owner, new SpecialCardReward(rewardCard, base.Owner));
                await CaptureSuccessPower.ApplyForCapture(base.Owner, rewardCard, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.CalculationBase.UpgradeValueBy(4m);
        base.DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }
}
