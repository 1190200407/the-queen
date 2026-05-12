using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>宣告：造成伤害并施加 2 回合内死亡可捕获的标记。</summary>
[Pool(typeof(QueenCardPool))]
public sealed class Declaration : QueenCardModel, ICanMonsterCapture
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public bool CanCapture(MonsterModel monster, CombatState combatState) =>
        monster is not null && combatState is not null
        && (combatState.Encounter?.RoomType switch
        {
            RoomType.Boss => false,
            RoomType.Elite => IsUpgraded,
            _ => true,
        });

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [QueenHoverTips.Capture];

    public Declaration()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        Creature target = cardPlay.Target;


        CombatState? combatState = target.CombatState ?? base.Owner.Creature.CombatState;
        if (target.Monster is not null && combatState is not null && CanCapture(target.Monster, combatState))
        {
            _ = await PowerCmd.Apply<DeclarationCaptureMarkPower>(
                target,
                2m,
                base.Owner.Creature,
                this);
        }
        else
        {
            _ = await PowerCmd.Apply<DeclarationCaptureMarkNoPower>(
                target,
                2m,
                base.Owner.Creature,
                this);
        }

        AttackCommand attackCommand = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        _ = attackCommand;
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
