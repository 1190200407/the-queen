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
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(TokenCardPool))]
public sealed class Devour : QueenCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    internal override bool HasSelfBound => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ModKeywordRegistry.GetCardKeyword(QueenKeyword.Fade)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(1m),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("HungerPower").WithMultiplier(static (CardModel card, Creature? _) =>
            card.Owner?.Creature?.GetPower<QueenHungerPower>() is { Amount: > 0 } h ? h.Amount : 0m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade),
        HoverTipFactory.FromPower<StrengthPower>(),
        .. HoverTipFactory.FromAffliction<Bound>(),
    ];

    public Devour()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        Creature target = cardPlay.Target;
        if (!target.IsAlive || base.Owner.Creature is not { IsAlive: true } applier)
        {
            return;
        }

        decimal strengthLoss = base.DynamicVars.Strength.BaseValue;
        await PowerCmd.Apply<DevourEnemyStrengthPower>(choiceContext, target, strengthLoss, applier, this);

        if (base.Owner.Creature.GetPower<QueenHungerPower>() is { Amount: > 0 } hunger)
        {
            await QueenCardCmd.AddSoulLamp(choiceContext, base.Owner, hunger.Amount);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Strength.UpgradeValueBy(1m);
    }
}
