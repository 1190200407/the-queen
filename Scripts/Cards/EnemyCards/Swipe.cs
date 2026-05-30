using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>偷窃：聚合体造成伤害、顺走目标对应怪物卡（<see cref="AmalgamSwipePower"/>），并启动/刷新逃跑倒计时（<see cref="AmalgamEscapePower"/>）。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class Swipe : QueenCardModel, ICanMonsterCapture
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    private const decimal escapeTurns = 3m;

    public bool CanCapture(MonsterModel monster, CombatState combatState) =>
        monster is not null && combatState is not null;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override int MaxUpgradeLevel => 0;
    internal override bool HasSelfBound => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new AmalgamLearnIntentDamageVar(17m, ValueProp.Move),
        new PowerVar<AmalgamEscapePower>(escapeTurns),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ..HoverTipFactory.FromAffliction<Bound>(),
        HoverTipFactory.FromPower<AmalgamSwipePower>(),
        HoverTipFactory.FromPower<AmalgamEscapePower>(),
    ];

    /// <summary>无友方聚合体或 <see cref="FriendlyAmalgam.BlockActionFromSleep"/> 时手牌红高亮（打出时由聚合体直接对敌伤害）。</summary>
    protected override bool ShouldGlowRedInternal =>
        (base.Owner?.Creature?.CombatState is { } combatState
            && (FriendlyAmalgamCmd.GetExisting(combatState, base.Owner) is not { Monster: FriendlyAmalgam amalgam }
                || amalgam.BlockActionFromSleep))
        || base.ShouldGlowRedInternal;

    public Swipe()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (base.Owner.Creature.CombatState is not { } combatState)
        {
            return;
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam == null)
        {
            return;
        }

        Creature target = cardPlay.Target;
        if (amalgam.Monster is FriendlyAmalgam fam && !fam.BlockActionFromSleep)
        {
            decimal damage = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
            if (target.IsAlive && damage > 0m)
            {
                AmalgamActionModel? attack = AmalgamActionRegistry.CreateOffense(damage, target);
                if (attack != null)
                {
                    await attack.ExecuteAsync(choiceContext, amalgam);
                }
            }
        }

        AmalgamSwipePower? swipe = amalgam.GetPower<AmalgamSwipePower>();
        if (swipe == null)
        {
            await PowerCmd.Apply<AmalgamSwipePower>(choiceContext, amalgam, 1m, base.Owner.Creature, this);
            swipe = amalgam.GetPower<AmalgamSwipePower>();
        }

        if (swipe != null)
        {
            bool success = await swipe.TryStealMonsterCaptureRewardAsync(base.Owner, target);
            if (!success)
            {
                return;
            }
        }

        await PowerCmd.Apply<AmalgamEscapePower>(choiceContext, amalgam, escapeTurns, base.Owner.Creature, this);
    }
}
