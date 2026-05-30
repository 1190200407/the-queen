using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>……无趣：造成伤害；可弃任意张手牌，每张获得 1 点力量。消耗。</summary>
[RegisterCard(typeof(QueenCardPool))]
public sealed class Joyless : QueenCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    public Joyless()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        Player? owner = base.Owner;
        CombatState? combat = base.CombatState;
        if (owner == null || combat == null || owner.Creature is not { IsAlive: true })
        {
            return;
        }

        IEnumerable<CardModel> selected = await CardSelectCmd.FromHandForDiscard(
            choiceContext,
            owner,
            new CardSelectorPrefs(new LocString("cards", "THE_QUEEN_CARD_JOYLESS.selectionPrompt"), 0, 999999999),
            c => c != this,
            this);

        IReadOnlyList<CardModel> handSnapshot = PileType.Hand.GetPile(owner).Cards;
        List<CardModel> toDiscard = selected
            .Distinct()
            .Where(c => IsValidHandDiscard(owner, combat, handSnapshot, c))
            .ToList();

        if (toDiscard.Count == 0)
        {
            return;
        }

        await CardCmd.Discard(choiceContext, toDiscard);
        await PowerCmd.Apply<StrengthPower>(choiceContext, owner.Creature, toDiscard.Count, owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);
    }

    private static bool IsValidHandDiscard(
        Player expectedOwner,
        CombatState currentCombat,
        IReadOnlyList<CardModel> hand,
        CardModel? card)
    {
        if (card == null || !ReferenceEquals(card.Owner, expectedOwner) || card.Owner?.Creature == null)
        {
            return false;
        }

        if (!hand.Contains(card))
        {
            return false;
        }

        CombatState? resolved = card.Owner.Creature.CombatState ?? card.CombatState;
        return resolved != null && ReferenceEquals(resolved, currentCombat);
    }
}
