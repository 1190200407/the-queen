using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>臣服于我：造成伤害；你的力量每超过目标 1 点，额外造成一次伤害�?/summary>
[RegisterCard(typeof(QueenCardPool))]
public sealed class SubmitToMe : QueenCardModel
{
    private const string calculatedHitsKey = "CalculatedHits";
    private const int strengthDiffPerExtraHit = 1;
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new CalculationBaseVar(1m),
        new CalculationExtraVar(1m),
        new CalculatedVar(calculatedHitsKey).WithMultiplier(static (CardModel card, Creature? target) =>
        {
            if (card.Owner?.Creature is not { } owner || target == null)
            {
                return 0m;
            }

            int diff = SubmitToMe.GetStrength(owner) - SubmitToMe.GetStrength(target);
            return diff > 0 ? diff / strengthDiffPerExtraHit: 0;
        }),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    public SubmitToMe()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (!cardPlay.Target.IsAlive || base.Owner.Creature is not { IsAlive: true })
        {
            return;
        }

        int hitCount = (int)((CalculatedVar)base.DynamicVars[calculatedHitsKey]).Calculate(cardPlay.Target);
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .WithHitCount(hitCount)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);
    }

    private static int GetStrength(Creature creature)
    {
        decimal amount = creature.GetPower<StrengthPower>()?.Amount ?? 0m;
        return (int)amount;
    }
}
