using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>你的旅程，到此为止：召唤后让聚合体沉睡 2 回合，随后获得力量。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class YourJoueneyEndsHere : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(9m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
        new PowerVar<StrengthPower>(10m)
    ];
    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    public YourJoueneyEndsHere()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);

        CombatState? combatState = base.Owner.Creature.CombatState;
        Creature? amalgamCreature = combatState != null ? FriendlyAmalgamCmd.GetExisting(combatState, base.Owner) : null;
        if (amalgamCreature is { IsAlive: true, Monster: FriendlyAmalgam amalgam })
        {
            await amalgam.FallAsleep(FriendlyAmalgam.SleepReason.YourTourEndsHere);

            YourJoueneyEndsHerePendingPower? pending = await PowerCmd.Apply<YourJoueneyEndsHerePendingPower>(
                amalgamCreature,
                2m,
                base.Owner.Creature,
                this);
            pending?.ConfigureStrength(base.DynamicVars.Strength.BaseValue);
        }
    }
}
