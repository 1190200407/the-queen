using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 硬化外壳（聚合体版）：本回合内，聚合体最多失去 <see cref="PowerModel.Amount"/> 点生命值；每回合开始重置。
/// （行为参考原版 Hardened Shell。）
/// </summary>
public sealed class AmalgamThieveryPower : QueenPowerModel, IAmalgamEventListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 复用原版 Hardened Shell 的图标资源。
    public override string? CustomPackedIconPath => "res://images/atlases/power_atlas.sprites/thievery_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/thievery_power.png";

    public async Task OnAmalgamDamagedCreatureAsync(
        CombatState combatState,
        PlayerChoiceContext choiceContext,
        Creature amalgam,
        Creature damagedEnemy,
        IEnumerable<DamageResult> damageResults)
    {
        _ = combatState;
        _ = choiceContext;
        _ = damagedEnemy;

        if (amalgam != base.Owner || !amalgam.IsAlive || Amount <= 0m)
        {
            return;
        }

        // 与原版“偷窃/盗窃”一类效果一致：只在“可被强化的攻击”造成未格挡伤害时结算。
        List<DamageResult> results = damageResults.ToList();
        if (results.Count == 0 || !results[0].Props.IsPoweredAttack())
        {
            return;
        }

        int hitCount = results.Count(static r => r.UnblockedDamage > 0);
        if (hitCount <= 0)
        {
            return;
        }

        decimal stolen = Amount * hitCount;
        Creature? applier = amalgam.PetOwner?.Creature;
        HeistPower? heist = amalgam.GetPower<HeistPower>();
        if (heist is null)
        {
            await PowerCmd.Apply<HeistPower>(amalgam, stolen, applier, cardSource: null);
        }
        else
        {
            await PowerCmd.ModifyAmount(heist, stolen, applier, cardSource: null);
        }
    }
}