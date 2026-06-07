using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>裂隙：0 费群体攻击；魂灯每层使本牌耗能 +1。</summary>
[RegisterCard(typeof(QueenCardPool))]
public sealed class Rift : QueenCardModel, ISoulLampEventListener
{
    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        base.EnergyHoverTip,
        HoverTipFactory.FromPower<SoulLampPower>(),
    ];

    public Rift()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override Task BeforeCombatStart()
    {
        RefreshEnergyCostDisplay();
        return base.BeforeCombatStart();
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card == this)
        {
            RefreshEnergyCostDisplay();
        }

        return base.AfterCardEnteredCombat(card);
    }

    public Task OnSoulLampAmountChanged(
        PlayerChoiceContext choiceContext,
        Player player,
        decimal delta,
        Creature? applier,
        CardModel? cardSource)
    {
        _ = choiceContext;
        _ = delta;
        _ = applier;
        _ = cardSource;
        if (player == base.Owner)
        {
            RefreshEnergyCostDisplay();
        }

        return Task.CompletedTask;
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this)
        {
            return false;
        }

        int soulLamp = GetOwnerSoulLampStacks();
        if (soulLamp <= 0)
        {
            return false;
        }

        modifiedCost = originalCost + soulLamp;
        return true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (base.CombatState is not CombatState combatState)
        {
            return;
        }

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(combatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(5m);
    }

    private void RefreshEnergyCostDisplay() => InvokeEnergyCostChanged();

    private int GetOwnerSoulLampStacks()
    {
        SoulLampPower? lamp = base.Owner?.Creature?.GetPower<SoulLampPower>();
        return lamp?.DisplayAmount ?? 0;
    }
}
