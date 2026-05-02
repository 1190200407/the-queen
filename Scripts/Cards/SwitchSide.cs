using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

[Pool(typeof(TokenCardPool))]
public sealed class SwitchSide : QueenCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = false;

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("IsLeft").WithMultiplier((CardModel card, Creature? _) => 
        {
            CombatState? combatState = card.Owner.Creature.CombatState;
            if (combatState is null)
            {
                return -1m;
            }
            foreach (Creature enemy in combatState.Enemies)
            {
                if (enemy.GetPower<BackAttackAmalgamPower>() != null)
                {
                    return 1m;
                }
                if (enemy.GetPower<BackAttackQueenPower>() != null)
                {
                    return 0m;
                }
            }
            return -1m;
        })
    ];

    public SwitchSide()
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

        Creature applier = base.Owner.Creature;
        foreach (Creature enemy in combatState.Enemies.ToArray())
        {
            if (!enemy.IsAlive)
            {
                continue;
            }

            if (enemy.GetPower<BackAttackQueenPower>() is { } queenBackAttack)
            {
                await PowerCmd.Remove(queenBackAttack);
                await PowerCmd.Apply<BackAttackAmalgamPower>(enemy, 1m, applier, this);
                continue;
            }
            if (enemy.GetPower<BackAttackAmalgamPower>() is { } amalgamBackAttack)
            {
                await PowerCmd.Remove(amalgamBackAttack);
                await PowerCmd.Apply<BackAttackQueenPower>(enemy, 1m, applier, this);
                continue;
            }

            // 没有任何后方攻击时：默认设为“受你造成的伤害 +50%”。
            await PowerCmd.Apply<BackAttackQueenPower>(enemy, 1m, applier, this);
        }
    }
}

