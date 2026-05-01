using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>库存：聚合体造成伤害；若手牌中没有其他库存，则从抽牌堆随机将 1 张库存置入手牌。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class Stock : QueenCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    private const decimal damage = 7m;

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new AmalgamLearnIntentDamageVar(damage, ValueProp.Move),
    ];

    public Stock()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (base.Owner.Creature.CombatState is not { } combatState)
        {
            return;
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is { IsAlive: true })
        {
            Creature target = cardPlay.Target;
            decimal dmg = base.DynamicVars["LearnIntentDamage"].BaseValue;
            if (target.IsAlive && dmg > 0m)
            {
                AmalgamActionModel? attack = AmalgamActionRegistry.CreateOffense(dmg, target);
                if (attack != null)
                {
                    await attack.ExecuteAsync(choiceContext, amalgam);
                }
            }
        }

        if (base.Owner.PlayerCombatState?.Hand is not { } hand)
        {
            return;
        }

        bool hasOtherStockInHand = hand.Cards.Any(c => c is Stock && !ReferenceEquals(c, this));
        if (hasOtherStockInHand)
        {
            return;
        }

        CardPile drawPile = PileType.Draw.GetPile(base.Owner);
        List<CardModel> stockInDraw = drawPile.Cards.Where(static c => c is Stock).ToList();
        if (stockInDraw.Count == 0 || base.Owner.RunState?.Rng.CombatCardSelection is not { } rng)
        {
            return;
        }

        if (rng.NextItem(stockInDraw) is not { } picked)
        {
            return;
        }

        await CardPileCmd.Add(picked, PileType.Hand);
    }
}
