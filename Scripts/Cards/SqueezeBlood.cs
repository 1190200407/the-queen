using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>榨干鲜血：目标负面效果不少于 3 个时伤害翻倍。</summary>
[RegisterCard(typeof(QueenCardPool))]
public sealed class SqueezeBlood : QueenCardModel
{
    private const int debuffThreshold = 3;
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move)];

    protected override bool ShouldGlowGoldInternal =>
        base.CombatState?.HittableEnemies.Any(HasEnoughDebuffsForDoubleDamage) ?? false;

    public SqueezeBlood()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (cardSource != this || dealer != base.Owner?.Creature || target == null)
        {
            return 1m;
        }

        return HasEnoughDebuffsForDoubleDamage(target) ? 2m : 1m;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);
    }

    private static bool HasEnoughDebuffsForDoubleDamage(Creature target) =>
        CountTargetDebuffs(target) >= debuffThreshold;

    private static int CountTargetDebuffs(Creature target) =>
        QueenDebuffUtil.CountDebuffPowers(target, excludeTemporary: true);
}
