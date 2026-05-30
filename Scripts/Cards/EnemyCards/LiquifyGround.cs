using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>液化地面：获得沙坑、召唤，并学习意图生成狂乱牵引。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class LiquifyGround : LearnIntentCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private const decimal sandpit = 7m;
    private const decimal summon = 20m;

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<AmalgamSandpitPower>(sandpit),
        new SummonVar(summon).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("Sandpit").WithMultiplier((CardModel card, Creature? _) =>
        {
            return card.Owner.Creature.GetPower<AmalgamSandpitPower>()?.Amount ?? 0m;
        }),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<AmalgamSandpitPower>(),
        HoverTipFactory.FromCard<FranticTug>(),
        ..base.AdditionalHoverTips,
    ];

    public LiquifyGround()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
    {
		if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1 && combatState.Encounter?.RoomType == RoomType.Boss)
        {
            base.DynamicVars.Power<AmalgamSandpitPower>().BaseValue += 3;
        }
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        SfxCmd.Play("event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_liquify_ground");
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		VfxCmd.PlayOnCreatureCenter(base.Owner.Creature, "vfx/vfx_scream");
		await Cmd.Wait(0.75f);

        decimal sandpitToGain = base.DynamicVars.Power<AmalgamSandpitPower>().BaseValue;
        if (base.Owner.Creature.GetPower<AmalgamSandpitPower>() == null)
        {
            await PowerCmd.Apply<AmalgamSandpitPower>(base.Owner.Creature, sandpitToGain, base.Owner.Creature, this);
        }
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, summon, this);

        await PlayLearnIntentsFromCreateAsync(choiceContext, cardPlay);
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        AmalgamActionModel intent = new AmalgamGenerateCardIntentAction<FranticTug>(1m);
        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}
