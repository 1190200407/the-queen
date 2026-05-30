using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(QueenCardPool))]
public sealed class GluttonousFeast : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    private const int devourCount = 2;
    private const int soulLampGain = 2;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<Devour>(upgrade: base.IsUpgraded),
        HoverTipFactory.FromPower<SoulLampPower>(),
        HoverTipFactory.FromPower<GluttonousFeastNoStrengthPower>(),
    ];

    public GluttonousFeast()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        Creature target = cardPlay.Target;
        if (!target.IsAlive || base.Owner.Creature is not { IsAlive: true } applier || base.CombatState == null)
        {
            return;
        }

        for (int i = 0; i < devourCount; i++)
        {
            await QueenCardCmd.CreateInHand<Devour>(base.Owner, base.CombatState, isUpgraded: base.IsUpgraded);
        }

        await QueenCardCmd.AddSoulLamp(choiceContext, base.Owner, soulLampGain);
        await PowerCmd.Apply<GluttonousFeastNoStrengthPower>(target, 1m, applier, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
