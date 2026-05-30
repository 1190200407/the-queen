using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>流电：获得流电。所有能力牌获得重放；消耗。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class Galvanic : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private const decimal galvanicStacks = 6m;
    private const decimal replay = 1m;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("GalvanicStacks", galvanicStacks),
        new IntVar("Replay", replay),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<GalvanicPower>(),
        ..HoverTipFactory.FromAffliction<Galvanized>(),
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic),
    ];

    public override int MaxUpgradeLevel => 0;

    public Galvanic()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        if (base.Owner.PlayerCombatState == null)
        {
            return;
        }

        int r = base.DynamicVars["Replay"].IntValue;
        if (r > 0)
        {
            foreach (CardModel c in base.Owner.PlayerCombatState.AllCards.Where(c => c.Type == CardType.Power).ToList())
            {
                c.BaseReplayCount += r;
            }
        }

        decimal stacks = base.DynamicVars["GalvanicStacks"].BaseValue;
        if (stacks > 0m)
        {
            await PowerCmd.Apply<GalvanicPower>(base.Owner.Creature, stacks, base.Owner.Creature, this);
        }
    }
}

