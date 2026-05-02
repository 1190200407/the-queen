using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

/// <summary>背刺：召唤并为所有敌人施加后方攻击，同时生成 1 张换边。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class BackAttack : QueenCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private const decimal summon = 20m;

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(summon).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        // 生成的换边牌提示（背刺本身是怪物牌，不是 Learn Intent）。
        HoverTipFactory.FromCard<SwitchSide>(),
    ];

    public BackAttack()
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

        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, summon, this);

        // 生成 1 张换边到手牌。
        await QueenCardCmd.CreateInHand<SwitchSide>(base.Owner, combatState);

        // 所有敌人获得后方攻击（默认：来自玩家的伤害 +50%）。
        Creature applier = base.Owner.Creature;
        foreach (Creature enemy in combatState.Enemies.ToArray())
        {
            if (!enemy.IsAlive)
            {
                continue;
            }
            await PowerCmd.Apply<BackAttackQueenPower>(enemy, 1m, applier, this);
        }
    }
}

