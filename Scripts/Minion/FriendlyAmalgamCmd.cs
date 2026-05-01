using System;
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

    /// <summary>击倒沉睡回合末：最大生命设为 1，当前生命置为 1。</summary>
    internal static async Task ApplyDeathSleepReviveStatsAsync(Creature creature)
    {
        CombatState? cs = creature.CombatState;
        if (cs == null)
        {
            return;
        }

        _ = cs;
        await CreatureCmd.SetMaxHp(creature, 1m);
        await CreatureCmd.SetCurrentHp(creature, 1m);
        SyncHealthBarVisibility(creature);
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

    /// <summary>战斗开场：仅生成 0 血的聚合体壳并写入固定最大生命，不治疗、不占召唤历史。</summary>
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
        await CreatureCmd.SetMaxHp(minion, 1m);
        TryTrackOwnerBlockOnAmalgamNode(minion);
        PlaceAmalgamByQueen(owner, minion);
        SyncHealthBarVisibility(minion);
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
            await amalgam.WakeUp(FriendlyAmalgam.SleepReason.Dead);
            amalgam.ClearForcedAction();
        }

        TryRefreshIntentTorchVisuals(minion);
        SyncHealthBarVisibility(minion);
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

    /// <summary>
    /// 学习灯槽意图的<strong>唯一入口</strong>：取友方聚合体；若当前生命为 0 则先 <see cref="EnsureOneHpViaSummonHealBeforeIntentAsync"/> 再写入。
    /// 卡牌/能力侧<strong>不要</strong>先 <see cref="GetExisting"/> 再以 <c>IsAlive</c> 短路，否则 0 血尸体态永远进不来这里。
    /// </summary>
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

        // 隐藏：当前生命为 0（含击倒沉睡尸体）时，先确保最大生命为 1 再 Heal 至多 1 点（与 Summon 铺血音效一致），再学意图。
        if (!amalgamCreature.IsAlive)
        {
            await Summon(choiceContext, owner, 1m, source);
        }

        await amalgamModel.LearnIntent(choiceContext, intent);
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
        IEnumerable<DamageResult> damageResults =
            await CreatureCmd.Damage(choiceContext, target, damage, ValueProp.Move, attacker, null);
        if (attacker.CombatState is { } combatState)
        {
            await FriendlyAmalgamHook.AfterAmalgamDamagedCreature(combatState, choiceContext, attacker, target, damageResults);
        }
    }

    /// <summary>
    /// 多段进攻：单次 <c>PowerAttack</c> 出手动画 + 光束音效；<c>Visuals/LaserControlBone</c> 固定向友方场侧偏移，随后按 <see cref="AmalgamOffenseTargeting"/> 连打 <paramref name="hitCount"/> 次。
    /// </summary>
    public static async Task ExecuteMultiHitOffense(
        PlayerChoiceContext choiceContext,
        Creature amalgam,
        decimal damagePerHit,
        int hitCount)
    {
        const string beamAnim = "PowerAttack";
        const float beamAnimDelay = 0.8f;
        const float damageDelayAfterBeamAnim = 0.5f;
        const string beamSfx = "event:/sfx/enemy/enemy_attacks/torch_head_amalgam/torch_head_amalgam_beam";
        const string hitVfxPath = "vfx/vfx_attack_blunt";
        const float laserControlBoneFriendlyReach = 10000f;

        if (hitCount <= 0 || damagePerHit <= 0m)
        {
            return;
        }

        CombatState? combatState = amalgam.CombatState;
        if (combatState == null || amalgam.PetOwner is not Player queen)
        {
            return;
        }

        Creature[] aliveInitial = combatState.Enemies.Where(e => e.IsAlive).ToArray();
        if (aliveInitial.Length == 0)
        {
            return;
        }

        TryOffsetMultiHitLaserControlBone(amalgam, laserControlBoneFriendlyReach);

        SfxCmd.Play(beamSfx);
        await CreatureCmd.TriggerAnim(amalgam, beamAnim, beamAnimDelay);
        await Cmd.CustomScaledWait(damageDelayAfterBeamAnim, damageDelayAfterBeamAnim);

        for (int i = 0; i < hitCount; i++)
        {
            Creature[] alive = combatState.Enemies.Where(e => e.IsAlive).ToArray();
            if (alive.Length == 0)
            {
                return;
            }

            AmalgamOffenseTargetingMode mode = AmalgamOffenseTargeting.ResolveMode(combatState, queen);
            if (mode == AmalgamOffenseTargetingMode.AllAliveEnemies)
            {
                foreach (Creature enemy in alive)
                {
                    VfxCmd.PlayOnCreatureCenter(enemy, hitVfxPath);
                }

                foreach (Creature enemy in alive)
                {
                    IEnumerable<DamageResult> damageResults =
                        await CreatureCmd.Damage(choiceContext, enemy, damagePerHit, ValueProp.Move, amalgam, null);
                    await FriendlyAmalgamHook.AfterAmalgamDamagedCreature(combatState, choiceContext, amalgam, enemy, damageResults);
                }

                continue;
            }

            if (mode == AmalgamOffenseTargetingMode.LockedMarkedEnemy)
            {
                Creature? marked = AmalgamOffenseTargeting.FindMarkedEnemy(combatState);
                if (marked is { IsAlive: true })
                {
                    VfxCmd.PlayOnCreatureCenter(marked, hitVfxPath);
                    IEnumerable<DamageResult> damageResults =
                        await CreatureCmd.Damage(choiceContext, marked, damagePerHit, ValueProp.Move, amalgam, null);
                    await FriendlyAmalgamHook.AfterAmalgamDamagedCreature(combatState, choiceContext, amalgam, marked, damageResults);
                    continue;
                }
            }

            Creature randomEnemy = alive[Random.Shared.Next(alive.Length)];
            VfxCmd.PlayOnCreatureCenter(randomEnemy, hitVfxPath);
            IEnumerable<DamageResult> randomHit =
                await CreatureCmd.Damage(choiceContext, randomEnemy, damagePerHit, ValueProp.Move, amalgam, null);
            await FriendlyAmalgamHook.AfterAmalgamDamagedCreature(combatState, choiceContext, amalgam, randomEnemy, randomHit);
        }
    }

    private static void TryOffsetMultiHitLaserControlBone(Creature amalgam, float reachAlongFriendlySide)
    {
        NCreature? nCreature = NCombatRoom.Instance?.GetCreatureNode(amalgam);
        if (nCreature == null)
        {
            return;
        }

        Node2D? laserBone = nCreature.GetSpecialNode<Node2D>("Visuals/LaserControlBone");
        if (laserBone == null)
        {
            return;
        }

        laserBone.Position += Vector2.Left * reachAlongFriendlySide;
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
            IEnumerable<DamageResult> damageResults =
                await CreatureCmd.Damage(choiceContext, enemy, damagePerEnemy, ValueProp.Move, amalgam, null);
            if (amalgam.CombatState is { } combatState)
            {
                await FriendlyAmalgamHook.AfterAmalgamDamagedCreature(combatState, choiceContext, amalgam, enemy, damageResults);
            }
        }
    }
}
