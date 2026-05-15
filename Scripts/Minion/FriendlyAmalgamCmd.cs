using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Random;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Logging;

namespace ComicChess.TheQueen;

public static class FriendlyAmalgamCmd
{
    private const float AmalgamScaleMaxHpReference = 150f;

    private const float AmalgamScaleTweenOnHpChange = 0.75f;

    /// <summary>意图锚点在 <see cref="NCreature"/> 局部空间中的 Y 夹紧（Y 轴向下）；只约束 <see cref="SyncAmalgamIntentContainerToIntentMarker"/>，不改 <see cref="NCreature.ScaleTo"/>。</summary>
    private const float AmalgamIntentAnchorLocalYMin = -530f;

    private const float AmalgamIntentAnchorLocalYMax = 80f;

    private const string AmalgamIntentSyncTweenMeta = "TheQueen_AmalgamIntentSyncTween";

    /// <summary>
    /// 仍在本场 <paramref name="combatState"/> 中的友方聚合体。逃跑等会 <c>RemoveCreature</c> 并清空 <c>Creature.CombatState</c>，
    /// 但原版不会从 <see cref="MegaCrit.Sts2.Core.Entities.Players.PlayerCombatState.Pets"/> 移除，故必须过滤，否则 <see cref="GetExisting"/> 会命中僵尸引用。
    /// </summary>
    public static Creature? GetExisting(CombatState combatState, Player owner)
    {
        return owner.Creature.Pets.FirstOrDefault(c =>
            c.Monster is FriendlyAmalgam && ReferenceEquals(c.CombatState, combatState));
    }

    /// <summary>与原版 <see cref="MegaCrit.Sts2.Core.Commands.CardCmd"/> / <see cref="MegaCrit.Sts2.Core.Commands.Builders.AttackCommand"/> 一致，使用女王的 <see cref="Rng.CombatTargets"/>，保证联机下随机选敌一致。</summary>
    public static Creature? NextRandomHittableEnemy(Player queen, IEnumerable<Creature> aliveEnemies) =>
        queen.RunState.Rng.CombatTargets.NextItem(aliveEnemies);

    private static async Task<Creature> AddAmalgamPetAsync(Player owner, MinionSummonOptions options)
    {
        Creature pet = await PlayerCmd.AddPet<FriendlyAmalgam>(owner);
        if (pet.Monster is FriendlyAmalgam amalgam)
        {
            await amalgam.OnSummon(owner, pet, options);
        }

        return pet;
    }

    /// <summary>与原版 <see cref="NCombatRoom.AddCreature"/> 里奥斯提分支一致：仅「本地视角下的该玩家」用右上偏移 + sibling 顺序；联机里其他玩家保持 <c>AddCreature</c> 已为随从排好的脚边一行。</summary>
    private static bool IsLayoutLocalPlayer(Player owner, CombatState? combatState)
    {
        if (LocalContext.IsMe(owner))
        {
            return true;
        }

        // 未挂 NetId 时 IsMe 恒 false（例如部分测试）；单人战斗仍视为本地主控。
        if (!LocalContext.NetId.HasValue && combatState != null && combatState.Players.Count == 1 && combatState.Players[0] == owner)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 对齐原版 <see cref="NCombatRoom.AddCreature"/> 奥斯提分支（本地主控）：<c>player.Position + GetOstyOffsetFromPlayer(pet)</c> 与 <c>MoveChild(p, player.GetIndex())</c>。
    /// 不用 <see cref="NCreature.OstyScaleToSize"/>：随后 <see cref="TryRefreshAmalgamScaleFromMaxHp"/> 的 <see cref="NCreature.ScaleTo"/> 会 Kill 同一 <c>_scaleTween</c>，打断奥斯提位移 tween。
    /// 联机里非本地玩家不调用本逻辑，保留 <c>AddCreature</c> 已为随从算好的脚边一行（<c>Y+10</c>）。
    /// 延迟一帧应用，避免 <c>Hitbox.Size</c> 尚未就绪导致 <see cref="NCreature.GetOstyOffsetFromPlayer"/> 偏差。
    /// </summary>
    private static void PlaceAmalgamByQueen(Player owner, Creature pet, CombatState? combatState)
    {
        if (!IsLayoutLocalPlayer(owner, combatState))
        {
            return;
        }

        void ApplyLocalAmalgamSlot()
        {
            if (NCombatRoom.Instance is not { } room)
            {
                return;
            }

            NCreature? o = room.GetCreatureNode(owner.Creature);
            NCreature? p = room.GetCreatureNode(pet);
            if (o == null || p == null || !GodotObject.IsInstanceValid(o) || !GodotObject.IsInstanceValid(p))
            {
                return;
            }

            p.Position = o.Position + NCreature.GetOstyOffsetFromPlayer(pet);
            p.GetParent().MoveChild(p, o.GetIndex());
            p.ToggleIsInteractable(true);
        }

        Callable.From(ApplyLocalAmalgamSlot).CallDeferred();
    }

    /// <summary>按 <see cref="Creature.MaxHp"/> 更新聚合体显示缩放（与奥斯提相同 <see cref="Osty.ScaleRange"/> 与 150 参考生命）；用 <see cref="NCreature.ScaleTo"/>，不移动节点位置。体型只增不减（当前血量变小时保持已有显示倍率）。</summary>
    public static void TryRefreshAmalgamScaleFromMaxHp(Creature amalgamCreature, float durationSeconds = AmalgamScaleTweenOnHpChange)
    {
        if (TestMode.IsOn || amalgamCreature.Monster is not FriendlyAmalgam)
        {
            return;
        }

        NCreature? node = NCombatRoom.Instance?.GetCreatureNode(amalgamCreature);
        if (node == null)
        {
            return;
        }

        float t = Mathf.Clamp((float)amalgamCreature.MaxHp / AmalgamScaleMaxHpReference, 0f, 1f);
        float fromHp = Mathf.Lerp(Osty.ScaleRange.X, Osty.ScaleRange.Y, t);
        float defaultScale = node.Visuals.DefaultScale;
        float currentMul = defaultScale > 0f ? node.Visuals.Scale.X / defaultScale : Osty.ScaleRange.X;
        float scaleMul = Mathf.Max(fromHp, currentMul);
        node.ScaleTo(scaleMul, durationSeconds);

        if (durationSeconds <= 0f)
        {
            Callable.From(() => SyncAmalgamIntentContainerToIntentMarker(node)).CallDeferred();
            return;
        }

        if (node.HasMeta(AmalgamIntentSyncTweenMeta))
        {
            Variant metaV = node.GetMeta(AmalgamIntentSyncTweenMeta);
            if (metaV.VariantType == Variant.Type.Object && metaV.AsGodotObject() is Tween oldTween && GodotObject.IsInstanceValid(oldTween))
            {
                oldTween.Kill();
            }

            node.RemoveMeta(AmalgamIntentSyncTweenMeta);
        }

        Tween intentSyncTween = node.CreateTween();
        node.SetMeta(AmalgamIntentSyncTweenMeta, intentSyncTween);
        intentSyncTween.Finished += () =>
        {
            if (GodotObject.IsInstanceValid(node) && node.HasMeta(AmalgamIntentSyncTweenMeta)
                && node.GetMeta(AmalgamIntentSyncTweenMeta).AsGodotObject() == intentSyncTween)
            {
                node.RemoveMeta(AmalgamIntentSyncTweenMeta);
            }
        };

        intentSyncTween.TweenMethod(
            Callable.From<float>(_ =>
            {
                if (GodotObject.IsInstanceValid(node))
                {
                    SyncAmalgamIntentContainerToIntentMarker(node);
                }
            }),
            0f,
            1f,
            durationSeconds);
    }

    private static void SyncAmalgamIntentContainerToIntentMarker(NCreature creatureNode)
    {
        Marker2D? marker = creatureNode.Visuals.GetNodeOrNull<Marker2D>("IntentPos");
        if (marker == null)
        {
            return;
        }

        Control intents = creatureNode.IntentContainer;
        Transform2D creatureGlobal = creatureNode.GetGlobalTransform();
        Vector2 anchor = creatureGlobal.AffineInverse() * marker.GlobalPosition;
        anchor.Y = Mathf.Clamp(anchor.Y, AmalgamIntentAnchorLocalYMin, AmalgamIntentAnchorLocalYMax);
        intents.Position = anchor - intents.Size / 2f;
    }

    public static async Task Summon(PlayerChoiceContext choiceContext, Player owner, decimal amount, AbstractModel? source)
    {
        CombatState? combatState = owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        if (AmalgamFledSummonBlock.IsSummonBlocked(combatState, owner))
        {
            return;
        }

        // 尚无「在场」友方聚合体且 Pets 里也没有任何 FriendlyAmalgam（含已逃跑的僵尸条目）时，若已有奥斯提实体（死灵等）：走 OstyCmd。
        if (GetExisting(combatState, owner) == null
            && !owner.Creature.Pets.Any(static c => c.Monster is FriendlyAmalgam)
            && owner.Osty != null)
        {
            await OstyCmd.Summon(choiceContext, owner, amount, source);
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
            TryRefreshAmalgamScaleFromMaxHp(existing);
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

        await FriendlyAmalgamHook.OnAmalgamEnterCombat(combatState, choiceContext, owner, minion);
        CombatManager.Instance.History.Summoned(combatState, (int)amount, owner);
        await EnsureAmalgamCorePowers(minion);
        TryTrackOwnerBlockOnAmalgamNode(minion);
        await Hook.AfterSummon(combatState, choiceContext, owner, amount);
        PlaceAmalgamByQueen(owner, minion, combatState);
        SyncHealthBarVisibility(minion);
        TryRefreshAmalgamScaleFromMaxHp(minion);
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
        if (creature.Monster is FriendlyAmalgam amalgam)
        {
            await amalgam.WakeUp(FriendlyAmalgam.SleepReason.Dead);
        }
        SyncHealthBarVisibility(creature);
        TryRefreshAmalgamScaleFromMaxHp(creature, 0f);
    }

    /// <summary>
    /// <see cref="FriendlyAmalgam.IsHealthBarVisible"/> 为真时仍可能被 <see cref="NCombatRoom.AddCreature"/> 里对随从统一 <c>ToggleIsInteractable(false)</c>、
    /// 或 <see cref="NCreature.AnimDie"/> 淡出血条影响；在聚合体复活/入场等时机调用以同步交互与血条 UI。
    /// </summary>
    public static void SyncHealthBarVisibility(Creature amalgamCreature)
    {
        if (amalgamCreature.Monster is not FriendlyAmalgam)
        {
            return;
        }

        NCreature? node = NCombatRoom.Instance?.GetCreatureNode(amalgamCreature);
        if (node == null || !GodotObject.IsInstanceValid(node))
        {
            return;
        }

        bool on = amalgamCreature.Monster.IsHealthBarVisible;
        node.ToggleIsInteractable(on);
        if (on)
        {
            _ = node.AnimEnableUi();
        }
    }

    /// <summary>战斗开场：仅生成 0 血的聚合体壳并写入固定最大生命，不治疗、不占召唤历史。</summary>
    public static async Task EnsureAmalgamCombatStartShellAsync(PlayerChoiceContext choiceContext, Player owner)
    {
        CombatState? combatState = owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        if (GetExisting(combatState, owner) != null)
        {
            return;
        }

        // 与 GetExisting 一致：栏内仅有已离场占位时不重复铺壳。
        if (owner.Creature.Pets.Any(static c => c.Monster is FriendlyAmalgam))
        {
            return;
        }

        Creature minion = await AddAmalgamPetAsync(owner, default);
        await EnsureAmalgamCorePowers(minion);
        await CreatureCmd.SetMaxHp(minion, 1m);
        await FriendlyAmalgamHook.OnAmalgamEnterCombat(combatState, choiceContext, owner, minion);
        TryTrackOwnerBlockOnAmalgamNode(minion);
        PlaceAmalgamByQueen(owner, minion, combatState);
        SyncHealthBarVisibility(minion);
        TryRefreshAmalgamScaleFromMaxHp(minion, 0f);
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

        // 顺序固定：先 DieForYou。0 血壳上第二段 Apply 依赖 AmalgamDieForYouPower.ShouldAllowHitting 在「尚无渴血」时对尸体短暂放行（见该处注释）。
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
    public static async Task LearnIntent(PlayerChoiceContext choiceContext, Player owner, AmalgamActionModel? intent, AbstractModel? source)
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
        await FriendlyAmalgamHook.AfterLearnIntent(combatState, choiceContext, owner, amalgamCreature, intent, source);
    }

    /// <summary>
    /// 合并意图的<strong>唯一入口</strong>：取友方聚合体；若当前生命为 0 则先 <see cref="Summon"/> 补至可行动再写入。
    /// 卡牌/能力侧<strong>不要</strong>先 <see cref="GetExisting"/> 再以 <c>IsAlive</c> 短路，否则 0 血尸体态永远进不来这里。
    /// </summary>
    public static async Task CombineIntent(PlayerChoiceContext choiceContext, Player owner, AmalgamActionModel? intent, AbstractModel? source, string? compositeIndexKey)
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

        if (!amalgamCreature.IsAlive)
        {
            await Summon(choiceContext, owner, 1m, source);
        }

        await amalgamModel.CombineIntentAsync(choiceContext, intent, compositeIndexKey);
        await FriendlyAmalgamHook.AfterCombineIntent(combatState, choiceContext, owner, amalgamCreature, intent, source, compositeIndexKey);
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
        const float beamAnimDelay = 0.7f;
        const float damageDelayAfterBeamAnim = 0.3f;
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

            Creature? randomEnemy = NextRandomHittableEnemy(queen, alive);
            if (randomEnemy is not { IsAlive: true })
            {
                continue;
            }

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
