using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>疯狂撕咬：聚合体对目标 2 连击；按目标负面效果数量召唤。</summary>
[Pool(typeof(QueenCardPool))]
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
        new SummonVar(4m).WithTooltip("QUEEN_SUMMON_DYNAMIC"),
    ];

    internal override bool HasSelfBound => true;

    public FrenziedBite()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature? target = cardPlay.Target;
        CombatState? combatState = base.Owner.Creature.CombatState;
        if (target == null || combatState == null)
        {
            return;
        }

        Creature? amalgamCreature = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgamCreature is { IsAlive: true, Monster: FriendlyAmalgam })
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
