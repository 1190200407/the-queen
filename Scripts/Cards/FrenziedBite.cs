using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Cards.DynamicVars;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>疯狂撕咬：聚合体对目�?2 连击；按目标负面效果数量召唤�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class FrenziedBite : QueenCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    private const int hitCount = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new SummonVar(4m).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedSummonTotal").WithMultiplier(static (CardModel card, Creature? target) =>
        {
            int debuffCount = target?.Powers.Count(static p => p.Type == PowerType.Debuff) ?? 0;
            return debuffCount * card.DynamicVars.Summon.BaseValue;
        }),
    ];

    internal override bool HasSelfBound => true;

    /// <summary>无友方聚合体�?<see cref="FriendlyAmalgam.BlockActionFromSleep"/> 时手牌红高亮（打出时由聚合体直接对敌伤害）�?/summary>
    protected override bool ShouldGlowRedInternal =>
        (base.Owner?.Creature?.CombatState is { } combatState
            && (FriendlyAmalgamCmd.GetExisting(combatState, base.Owner) is not { Monster: FriendlyAmalgam amalgam }
                || amalgam.BlockActionFromSleep))
        || base.ShouldGlowRedInternal;

    public FrenziedBite()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature? target = cardPlay.Target;
        ICombatState? combatState = base.Owner.Creature.CombatState;
        if (target == null || combatState == null)
        {
            return;
        }

        Creature? amalgamCreature = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgamCreature is { Monster: FriendlyAmalgam fam } && !fam.BlockActionFromSleep)
        {
            decimal damagePerHit = base.DynamicVars.Damage.BaseValue;
            for (int i = 0; i < hitCount; i++)
            {
                if (!target.IsAlive)
                {
                    break;
                }

                await FriendlyAmalgamCmd.ExecuteSingleTargetAttack(
                    choiceContext,
                    amalgamCreature,
                    target,
                    damagePerHit,
                    "Attack",
                    0.6f,
                    "vfx/vfx_attack_blunt");
            }
        }

        int debuffCount = target.Powers.Count(static p => p.Type == PowerType.Debuff);
        if (debuffCount > 0)
        {
            decimal summonPerDebuff = base.DynamicVars.Summon.BaseValue;
            await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, summonPerDebuff * debuffCount, this);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);
        base.DynamicVars.Summon.UpgradeValueBy(1m);
    }
}
