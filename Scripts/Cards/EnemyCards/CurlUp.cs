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
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>蜷身：使聚合体获得蜷身，并学习「攻击 + 本回合失去力量」意图。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class CurlUp : QueenCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<AmalgamCurlUpPower>(10m),
        new AmalgamLearnIntentDamageVar(6m, ValueProp.Move),
        new IntVar("LearnIntentStrengthLoss", 2m),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        QueenHoverTips.LearnIntent,
        HoverTipFactory.FromPower<AmalgamCurlUpPower>(),
    ];

    public override int MaxUpgradeLevel => 0;

    public CurlUp()
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

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is { IsAlive: true })
        {
            decimal amount = base.DynamicVars.Power<AmalgamCurlUpPower>().BaseValue;
            if (amount > 0m)
            {
                await PowerCmd.Apply<AmalgamCurlUpPower>(amalgam, amount, base.Owner.Creature, this);
            }

            decimal dmg = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
            decimal strLoss = base.DynamicVars["LearnIntentStrengthLoss"].BaseValue;
            if (dmg > 0m && strLoss > 0m)
            {
                await FriendlyAmalgamCmd.LearnIntent(
                    choiceContext,
                    base.Owner,
                    new AmalgamAttackAndStrengthDownIntentAction(dmg, strLoss),
                    this);
            }
        }
    }
}
