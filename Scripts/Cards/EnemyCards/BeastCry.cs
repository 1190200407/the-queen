using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

/// <summary>兽吼：学习晕眩意图，消耗�?/summary>
[Pool(typeof(EnemyCardPool))]
public sealed class BeastCry : LearnIntentCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(15m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
    ];

    public BeastCry()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        AmalgamActionModel intent = new AmalgamSpecialIntentAction(
            moveId: "AMALGAM_INTENT_SPECIAL_BEAST_CRY",
            intentDescriptionKey: "AMALGAM_SPECIAL_BEAST_CRY.description",
            execute: async (PlayerChoiceContext _, Creature amalgam, Creature owner) =>
            {
                CombatState? cs = amalgam.CombatState;
                if (cs == null)
                {
                    return;
                }

                await CreatureCmd.TriggerAnim(amalgam, "Cast", 0f);

                Creature[] alive = cs.Enemies.Where(e => e.IsAlive).ToArray();
                if (alive.Length > 0)
                {
                    foreach (Creature enemy in alive)
                    {
                        await CreatureCmd.Stun(enemy);
                    }
                }

                await Cmd.CustomScaledWait(1.5f, 2f);
                await PowerCmd.Apply<AmalgamSleepPower>(amalgam, 2m, applier: owner, cardSource: null);
            });

        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }

    protected override decimal GetSummonAmount(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        base.DynamicVars.Summon.BaseValue;

    protected override bool ShouldSummonBeforeLearnIntent => true;
}
