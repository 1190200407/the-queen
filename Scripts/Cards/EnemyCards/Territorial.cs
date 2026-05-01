using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>领地意识：召唤并给予女王「领地意识」能力。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class Territorial : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(9m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    internal override bool HasSelfBound => true;
    public override int MaxUpgradeLevel => 0;

    public Territorial()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);
        await PowerCmd.Apply<TerritorialAwarenessPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
    }
}
