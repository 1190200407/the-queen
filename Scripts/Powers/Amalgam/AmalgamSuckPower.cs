using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Players;

namespace ComicChess.TheQueen;

/// <summary>吮吸（聚合体）：聚合体造成“可被强化的攻击”伤害后，按命中次数获得力量。</summary>
public sealed class AmalgamSuckPower : QueenPowerModel, IAmalgamEventListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 复用原版 Suck 的图标资源。
    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/suck_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/suck_power.png";

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

        // 对齐原版 SuckPower：只在“可被强化的攻击”触发。
        List<DamageResult> results = damageResults.ToList();
        if (results.Count == 0 || !results[0].Props.IsPoweredAttack())
        {
            return;
        }

        // 原版处理：若命中的是宠物，会同时生成宠物与主人的 DamageResult；需要去重避免算两次。
        List<DamageResult> petHits = results.Where(static r => r.Receiver.IsPet).ToList();
        foreach (DamageResult petHit in petHits)
        {
            Creature? owner = petHit.Receiver.PetOwner?.Creature;
            if (owner != null)
            {
                results.RemoveAll(r => r.Receiver == owner);
            }
        }

        int hitCount = results.Count(static r => r.UnblockedDamage > 0);
        if (hitCount <= 0)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<StrengthPower>(amalgam, Amount * hitCount, amalgam, null);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await PowerCmd.Remove(this);
    }
}

