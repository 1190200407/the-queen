using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>锁魂匣：保留；斩杀（<see cref="StaticHoverTip.Fatal"/>）；捕获见 <see cref="QueenHoverTips.Capture"/>；成功时施加 <see cref="CaptureSuccessPower"/>（奖励 <see cref="GrantOffense"/>）。</summary>
[Pool(typeof(QueenCardPool))]
public sealed class SoulLockCasket : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Fatal),
        QueenHoverTips.Capture,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];

    public SoulLockCasket()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        Creature target = cardPlay.Target;
        bool shouldTriggerFatal = target.Powers.All(static p => p.ShouldOwnerDeathTriggerFatal());
        AttackCommand attackCommand = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        
        CombatRoom? room = base.CombatState?.RunState.CurrentRoom as CombatRoom;
        if (room is null)
        {
            return;
        }
        
        if (shouldTriggerFatal
            && attackCommand.Results.Any(static r => r.WasTargetKilled)
            && target.CombatState?.Encounter?.RoomType != RoomType.Boss
            && base.CombatState?.RunState.CurrentRoom is CombatRoom)
        {
            GrantOffense grantOffense = base.Owner.RunState.CreateCard<GrantOffense>(base.Owner);
            room.AddExtraReward(base.Owner, new SpecialCardReward(grantOffense, base.Owner));
            await CaptureSuccessPower.ApplyForCapture(base.Owner, grantOffense, this);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
