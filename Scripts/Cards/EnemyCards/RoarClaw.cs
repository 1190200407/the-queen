using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>咆哮爪击：聚合体先施加易伤，再学习 2 连击进攻意图。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class RoarClaw : QueenCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    private const int learnIntentRepeat = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new AmalgamLearnIntentVulnerableVar(3m),
        new AmalgamLearnIntentDamageVar(5m, ValueProp.Move),
    ];
    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        QueenHoverTips.LearnIntent,
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    public RoarClaw()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        CombatState? combatState = base.Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        Creature? amalgamCreature = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgamCreature is { IsAlive: true, Monster: FriendlyAmalgam })
        {
            decimal stacks = base.DynamicVars["LearnIntentVulnerable"].BaseValue;
            Creature? selectedEnemy = cardPlay.Target is { IsAlive: true } t ? t : null;
            AmalgamActionModel? vulnerableAction = AmalgamActionRegistry.CreateVulnerable(stacks, selectedEnemy);
            if (vulnerableAction != null)
            {
                await vulnerableAction.ExecuteAsync(choiceContext, amalgamCreature);
            }
        }

        decimal dmg = base.DynamicVars["LearnIntentDamage"].BaseValue;
        AmalgamActionModel? intent = AmalgamActionRegistry.CreateOffenseMulti(dmg, learnIntentRepeat);
        await FriendlyAmalgamCmd.LearnIntent(choiceContext, base.Owner, intent, this);
    }
}
