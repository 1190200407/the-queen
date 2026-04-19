using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

public static class FriendlyAmalgamCmd
{
    public static Creature? GetExisting(CombatState combatState, Player owner)
    {
        return combatState.Allies.FirstOrDefault(c =>
            c.PetOwner == owner && c.Monster is FriendlyAmalgam);
    }

    private static async Task<Creature> AddAmalgamPetAsync(Player owner, MinionSummonOptions options)
    {
        Creature pet = await PlayerCmd.AddPet<FriendlyAmalgam>(owner);
        if (pet.Monster is FriendlyAmalgam amalgam)
        {
            await amalgam.OnSummon(owner, pet, options);
        }

        return pet;
    }

    /// <summary>聚合体相对女王本体节点固定偏移（单仆从，无 layout）。</summary>
    private static void PlaceAmalgamByQueen(Player owner, Creature pet)
    {
        if (NCombatRoom.Instance is not { } room)
        {
            return;
        }

        NCreature? o = room.GetCreatureNode(owner.Creature);
        NCreature? p = room.GetCreatureNode(pet);
        if (o == null || p == null)
        {
            return;
        }

        p.Position = o.Position + new Vector2(320f, -75f);
    }

    public static async Task Summon(PlayerChoiceContext choiceContext, Player owner, decimal amount, AbstractModel? source)
    {
        CombatState? combatState = owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        amount = Hook.ModifySummonAmount(combatState, owner, amount, source);
        if (amount <= 0m)
        {
            return;
        }

        Creature? existing = GetExisting(combatState, owner);

        if (existing is { IsAlive: true })
        {
            await CreatureCmd.GainMaxHp(existing, amount);
            CombatManager.Instance.History.Summoned(combatState, (int)amount, owner);
            await EnsureAmalgamCorePowers(existing);
            TryTrackOwnerBlockOnAmalgamNode(existing);
            return;
        }

        bool isReviving = existing != null;
        Creature minion = existing ?? await AddAmalgamPetAsync(
            owner,
            new MinionSummonOptions(Source: source as CardModel));
        
        if (isReviving)
        {
            owner.PlayerCombatState?.AddPetInternal(minion);
        }

        if (isReviving)
        {
            await CreatureCmd.SetMaxHp(minion, amount);
            await HealCurrentUpToSummonTargetAsync(minion, amount);
            await FinishSummonRevivePresentationAsync(minion);
        }
        else
        {
            await CreatureCmd.SetMaxHp(minion, amount);
            await HealCurrentUpToSummonTargetAsync(minion, amount);
        }
        CombatManager.Instance.History.Summoned(combatState, (int)amount, owner);
        await EnsureAmalgamCorePowers(minion);
        TryTrackOwnerBlockOnAmalgamNode(minion);
        await Hook.AfterSummon(combatState, choiceContext, owner, amount);
        PlaceAmalgamByQueen(owner, minion);
        SyncHealthBarVisibility(minion);
    }

    /// <summary><see cref="FriendlyAmalgam.IsHealthBarVisible"/> 在节点 <c>_Ready</c> 后若存活状态变化，须调此以同步 <see cref="NCreature.ToggleIsInteractable"/>（否则血条可见性会停留在旧状态）。</summary>
    public static void SyncHealthBarVisibility(Creature amalgamCreature)
    {
        if (amalgamCreature.Monster is not FriendlyAmalgam)
        {
            return;
        }

        NCombatRoom.Instance?.GetCreatureNode(amalgamCreature)?.ToggleIsInteractable(amalgamCreature.Monster.IsHealthBarVisible);
    }

    /// <summary>战斗开场：仅生成 0 血的聚合体壳并写入按遭遇缩放的最大生命（与奥斯提复活时重算 Max 一致），不治疗、不占召唤历史。</summary>
    public static async Task EnsureAmalgamCombatStartShellAsync(PlayerChoiceContext choiceContext, Player owner)
    {
        _ = choiceContext;
        CombatState? combatState = owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        if (GetExisting(combatState, owner) != null)
        {
            return;
        }

        Creature minion = await AddAmalgamPetAsync(owner, default);
        await EnsureAmalgamCorePowers(minion);
        await CreatureCmd.SetMaxHp(minion, ScaledSummonBaseHp(combatState, 1m));
        TryTrackOwnerBlockOnAmalgamNode(minion);
        PlaceAmalgamByQueen(owner, minion);
        SyncHealthBarVisibility(minion);
    }

    /// <summary>与 <see cref="MegaCrit.Sts2.Core.Commands.OstyCmd.Summon"/> 一致：用 <see cref="Creature.ScaleHpForMultiplayer"/> 把「召唤基数」缩放到当前遭遇人数/位面。</summary>
    public static decimal ScaledSummonBaseHp(CombatState combatState, decimal baseHp) =>
        System.Math.Max(1m, Creature.ScaleHpForMultiplayer(
            baseHp,
            combatState.Encounter,
            combatState.Players.Count,
            combatState.RunState.CurrentActIndex));

    /// <summary>击倒沉睡回合末：重算最大生命至缩放基数，当前生命置为 1（不超过最大）。</summary>
    internal static async Task ApplyDeathSleepReviveStatsAsync(Creature creature)
    {
        CombatState? cs = creature.CombatState;
        if (cs == null)
        {
            return;
        }

        decimal max = ScaledSummonBaseHp(cs, 1m);
        await CreatureCmd.SetMaxHp(creature, max);
        await CreatureCmd.SetCurrentHp(creature, System.Math.Min(1m, max));
        SyncHealthBarVisibility(creature);
    }

    /// <summary>在已写入的最大生命下用 <see cref="CreatureCmd.Heal"/> 补足当前生命至目标值（含治疗/召唤类音效）。</summary>
    private static async Task HealCurrentUpToSummonTargetAsync(Creature minion, decimal targetCurrentHp)
    {
        decimal need = targetCurrentHp - minion.CurrentHp;
        if (need > 0m)
        {
            await CreatureCmd.Heal(minion, need, playAnim: true);
        }
    }

    private static async Task FinishSummonRevivePresentationAsync(Creature minion)
    {
        minion.GetPower<AmalgamDieForYouPower>()?.WakeImmediatelyAfterSummonRevive();
        if (minion.Monster is FriendlyAmalgam amalgam)
        {
            amalgam.ClearForcedAction();
        }

        await AwakeAsync(minion);
        TryRefreshIntentTorchVisuals(minion);
        SyncHealthBarVisibility(minion);
    }

    public static async Task AwakeAsync(Creature creature)
    {
        if (creature.Monster is not FriendlyAmalgam || !creature.IsAlive)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(creature, "Idle", 0f);
    }

    /// <summary>与奥斯提一致：随从血条跟随主人的格挡状态（有格挡时血条呈护盾色）。</summary>
    private static void TryTrackOwnerBlockOnAmalgamNode(Creature amalgam)
    {
        if (amalgam.Monster is not FriendlyAmalgam || amalgam.PetOwner is not { Creature: { } owner })
        {
            return;
        }

        NCombatRoom.Instance?.GetCreatureNode(amalgam)?.TrackBlockStatus(owner);
    }

    private static async Task EnsureAmalgamCorePowers(Creature minion)
    {
        if (minion.Monster is not FriendlyAmalgam)
        {
            return;
        }

        if (minion.GetPower<AmalgamDieForYouPower>() == null)
        {
            await PowerCmd.Apply<AmalgamDieForYouPower>(minion, 1m, null, null);
        }

        if (minion.GetPower<AmalgamEvolutionaryThirstPower>() == null)
        {
            await PowerCmd.Apply<AmalgamEvolutionaryThirstPower>(minion, 1m, null, null);
        }
    }

    public static async Task LearnIntent(
        PlayerChoiceContext choiceContext,
        Player owner,
        AmalgamActionModel? intent,
        AbstractModel? source)
    {
        if (intent == null)
        {
            return;
        }

        CombatState? combatState = owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        Creature? amalgamCreature = GetExisting(combatState, owner);
        if (amalgamCreature?.Monster is not FriendlyAmalgam amalgamModel)
        {
            return;
        }

        // 隐藏：当前生命为 0（含击倒沉睡尸体）时，先重算最大生命再 Heal 至多 1 点（与 Summon 铺血音效一致），再学意图。
        await EnsureOneHpViaSummonHealBeforeIntentAsync(amalgamCreature, combatState);
        if (!amalgamCreature.IsAlive)
        {
            return;
        }

        await amalgamModel.LearnIntent(choiceContext, intent);
    }

    /// <summary>学习意图前若生命为 0：按遭遇重算最大生命，再 <see cref="CreatureCmd.Heal"/> 至多 1 点（含音效），并清除击倒沉睡占位与强制沉睡展示。</summary>
    private static async Task EnsureOneHpViaSummonHealBeforeIntentAsync(Creature amalgam, CombatState combatState)
    {
        if (amalgam.CurrentHp > 0m)
        {
            return;
        }

        decimal max = ScaledSummonBaseHp(combatState, 1m);
        await CreatureCmd.SetMaxHp(amalgam, max);
        decimal room = max - amalgam.CurrentHp;
        if (room <= 0m)
        {
            return;
        }

        await CreatureCmd.Heal(amalgam, System.Math.Min(1m, room), playAnim: true);
        await FinishSummonRevivePresentationAsync(amalgam);
    }

    /// <summary>将 <see cref="FriendlyAmalgam"/> 三槽意图与 <see cref="NewNAmalgamVfx"/> 小火同步（无节点时静默跳过）。</summary>
    public static void TryRefreshIntentTorchVisuals(Creature creature)
    {
        if (creature.Monster is not FriendlyAmalgam amalgam)
        {
            return;
        }

        // NAmalgamVfx 挂在 Spine 子节点 Visuals 下，不是 NCreatureVisuals 根的直接子节点。
        NewNAmalgamVfx? vfx = NCombatRoom.Instance?.GetCreatureNode(creature)?.Visuals.GetNodeOrNull<NewNAmalgamVfx>("Visuals/NAmalgamVfx");
        vfx?.SyncIntentSlotsFromAmalgam(amalgam);
    }

    /// <summary>聚合体在回合末执行<strong>灯槽内已记录</strong>的意图之前调用；满槽当场学习执行的路径不要调用。</summary>
    public static async Task TryPerformIntent(Creature attacker)
    {
        var attackerNode = NCombatRoom.Instance?.GetCreatureNode(attacker);
        if (attackerNode != null)
        {
            await attackerNode.PerformIntent();
        }
    }

    public static async Task ExecuteSingleTargetAttack(
        PlayerChoiceContext choiceContext,
        Creature attacker,
        Creature target,
        decimal damage,
        string attackerAnimName,
        float attackerAnimDelay,
        string hitVfxPath)
    {
        await CreatureCmd.TriggerAnim(attacker, attackerAnimName, attackerAnimDelay);
        VfxCmd.PlayOnCreatureCenter(target, hitVfxPath);
        await CreatureCmd.Damage(choiceContext, target, damage, ValueProp.Move, attacker, null);
    }

    /// <summary>万灵破军等：只播一次出手动画，再对多名敌人依次受击 VFX 与 <see cref="CreatureCmd.Damage"/>（非多次单体连打）。</summary>
    public static async Task ExecuteAllEnemiesSingleSwingAttack(
        PlayerChoiceContext choiceContext,
        Creature amalgam,
        IReadOnlyList<Creature> aliveEnemies,
        decimal damagePerEnemy,
        string attackerAnimName,
        float attackerAnimDelay,
        string hitVfxPath)
    {
        if (aliveEnemies.Count == 0)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, attackerAnimName, attackerAnimDelay);
        foreach (Creature enemy in aliveEnemies)
        {
            VfxCmd.PlayOnCreatureCenter(enemy, hitVfxPath);
        }

        foreach (Creature enemy in aliveEnemies)
        {
            await CreatureCmd.Damage(choiceContext, enemy, damagePerEnemy, ValueProp.Move, amalgam, null);
        }
    }
}
