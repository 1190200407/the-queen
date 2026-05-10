using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

public interface IAmalgamEventListener
{
    Task OnAmalgamDamagedCreatureAsync(
        CombatState combatState,
        PlayerChoiceContext choiceContext,
        Creature amalgam,
        Creature damagedEnemy,
        IEnumerable<DamageResult> damageResults)
    {
        return Task.CompletedTask;
    }

    Task OnAmalgamActAsync(CombatState combatState, PlayerChoiceContext choiceContext, Creature amalgam)
    {
        return Task.CompletedTask;
    }

    Task OnAmalgamWakeFromSleepAsync(CombatState combatState, Creature amalgam)
    {
        return Task.CompletedTask;
    }

    Task OnAmalgamEscapeAsync(CombatState combatState, Creature amalgam)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 聚合体被命中（产生未格挡伤害）时触发。用于“胆小”等需要在被命中瞬间响应的能力。
    /// </summary>
    Task OnAmalgamHitAsync(
        CombatState combatState,
        Creature amalgam,
        decimal unblockedDamage,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return Task.CompletedTask;
    }

    Task AfterAmalgamTurnEnd(CombatState combatState, Creature amalgam)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 友方聚合体在战斗中完成一次「基准最大生命」写入后（开场壳或召唤/复活 <see cref="CreatureCmd.SetMaxHp"/>），用于叠加跨局加成等。
    /// </summary>
    Task OnAmalgamEnterCombat(
        CombatState combatState,
        PlayerChoiceContext? choiceContext,
        Player owner,
        Creature amalgam)
    {
        return Task.CompletedTask;
    }

    /// <summary><see cref="FriendlyAmalgamCmd.LearnIntent"/> 完成写入之后。</summary>
    Task AfterLearnIntent(
        CombatState combatState,
        PlayerChoiceContext choiceContext,
        Player amalgamOwner,
        Creature amalgam,
        AmalgamActionModel intent,
        AbstractModel? source)
    {
        return Task.CompletedTask;
    }

    /// <summary><see cref="FriendlyAmalgamCmd.CombineIntent"/> 完成写入之后。</summary>
    Task AfterCombineIntent(
        CombatState combatState,
        PlayerChoiceContext choiceContext,
        Player amalgamOwner,
        Creature amalgam,
        AmalgamActionModel intent,
        AbstractModel? source,
        string? compositeIndexKey)
    {
        return Task.CompletedTask;
    }
}