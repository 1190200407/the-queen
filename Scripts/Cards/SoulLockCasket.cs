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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>锁魂匣：保留；斩杀（<see cref="StaticHoverTip.Fatal"/>）；捕获见 <see cref="QueenHoverTips.Capture"/>；成功时按 <see cref="MonsterCaptureRewardCatalog"/> 施加 <see cref="CaptureSuccessPower"/>（无配置则无奖励）。</summary>
[RegisterCharacterStarterCard(typeof(QueenCharacter), 1)]
[RegisterArchaicToothTranscendence(typeof(SoulCalmCasket))]
public sealed class SoulLockCasket : QueenCardModel, ICanMonsterCapture
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public bool CanCapture(MonsterModel monster, CombatState combatState) =>
        monster is not null && combatState is not null && combatState.Encounter?.RoomType != RoomType.Boss;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];

    public SoulLockCasket()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    
    public CardModel GetTranscendenceTransformedCard()
    {
        return ModelDb.Card<SoulCalmCasket>();
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

        CombatRoom? combatRoom = base.CombatState?.RunState.CurrentRoom as CombatRoom;
        if (combatRoom is null)
        {
            return;
        }

        if (target.Monster is not null && base.CombatState is not null && !CanCapture(target.Monster, base.CombatState))
        {
            return;
        }

        if (shouldTriggerFatal
            && attackCommand.Results.Any(static r => r.WasTargetKilled))
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
        AddKeyword(CardKeyword.Retain);
        base.DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
