using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>饥饿（噬尸蛞蝓）：召唤、使聚合体获得饥饿，并学习进攻意图。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class CorpseSlugHunger : QueenCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(10m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new IntVar("RavenousStacks", 4m),
        new AmalgamLearnIntentDamageVar(6m, ValueProp.Move),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        QueenHoverTips.LearnIntent,
        HoverTipFactory.FromPower<AmalgamRavenousPower>(),
    ];

    public override int MaxUpgradeLevel => 0;

    public CorpseSlugHunger()
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

        decimal summon = base.DynamicVars.Summon.BaseValue;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, summon, this);

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is { IsAlive: true })
        {
            decimal stacks = base.DynamicVars["RavenousStacks"].BaseValue;
            if (stacks > 0m)
            {
                await PowerCmd.Apply<AmalgamRavenousPower>(amalgam, stacks, base.Owner.Creature, this);
            }

            decimal dmg = base.DynamicVars["LearnIntentDamage"].BaseValue;
            AmalgamActionModel? intent = AmalgamActionRegistry.CreateOffense(dmg);
            await FriendlyAmalgamCmd.LearnIntent(choiceContext, base.Owner, intent, this);
        }
    }
}

