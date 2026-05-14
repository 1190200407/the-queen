using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

[Pool(typeof(TokenCardPool))]
public sealed class Nibble : QueenCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = false;

    private const decimal damage = 4m;

    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [QueenKeyword.fade];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new AmalgamLearnIntentDamageVar(damage, ValueProp.Move),
    ];

    /// <summary>无友方聚合体或 <see cref="FriendlyAmalgam.BlocksDirectOffenseFromHand"/> 时手牌红高亮（与 <see cref="Stock"/> 等一致）。</summary>
    protected override bool ShouldGlowRedInternal =>
        (base.Owner?.Creature?.CombatState is { } combatState
            && (FriendlyAmalgamCmd.GetExisting(combatState, base.Owner) is not { Monster: FriendlyAmalgam amalgam }
                || amalgam.BlocksDirectOffenseFromHand))
        || base.ShouldGlowRedInternal;

    public Nibble()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (base.Owner.Creature is not { IsAlive: true })
        {
            return;
        }

        if (base.Owner.Creature.CombatState is not { } combatState)
        {
            return;
        }

        if (FriendlyAmalgamCmd.GetExisting(combatState, base.Owner) is not { Monster: FriendlyAmalgam fam } amalgam
            || fam.BlocksDirectOffenseFromHand)
        {
            return;
        }

        Creature target = cardPlay.Target;
        if (!target.IsAlive)
        {
            return;
        }

        decimal dmg = AmalgamLearnIntentDamageVar.GetEffectiveFlatForOffenseIntent(this, "LearnIntentDamage");
        if (dmg <= 0m)
        {
            return;
        }

        AmalgamActionModel? attack = AmalgamActionRegistry.CreateOffense(dmg, target);
        if (attack != null)
        {
            await attack.ExecuteAsync(choiceContext, amalgam);
        }
    }
}
