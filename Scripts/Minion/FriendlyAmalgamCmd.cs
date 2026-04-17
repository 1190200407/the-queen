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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Commands;
using MinionLib.Layout;
using MinionLib.Minion;

namespace ComicChess.TheQueen;

public static class FriendlyAmalgamCmd
{
    public static Creature? GetExisting(CombatState combatState, Player owner)
    {
        return combatState.Allies.FirstOrDefault(c =>
            c.PetOwner == owner && c.Monster is FriendlyAmalgam);
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
            return;
        }

        bool isReviving = existing != null;
        Creature minion = existing ?? await MinionCmd.AddMinion<FriendlyAmalgam>(
            owner,
            new MinionSummonOptions(Source: source as CardModel, Position: MinionPosition.Front));
        
        if (isReviving)
        {
            owner.PlayerCombatState?.AddPetInternal(minion);
        }

        if (isReviving)
        {
            // 复活已有聚合体：直接把最大生命与当前生命一起设置到指定值，保留治疗类表现。
            await CreatureCmd.SetMaxAndCurrentHp(minion, amount);
        }
        else
        {
            // 首次召唤：只设置最大生命与当前生命，避免播放治疗音效。
            await CreatureCmd.SetMaxHp(minion, amount);
            await CreatureCmd.SetCurrentHp(minion, amount);
        }
        CombatManager.Instance.History.Summoned(combatState, (int)amount, owner);
        await Hook.AfterSummon(combatState, choiceContext, owner, amount);
    }

    public static async Task LearnIntent(Player owner, AmalgamActionModel? intent, AbstractModel? source)
    {
        _ = source;
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
        if (amalgamCreature is not { IsAlive: true })
        {
            return;
        }

        if (amalgamCreature.Monster is not FriendlyAmalgam amalgamModel)
        {
            return;
        }

        await amalgamModel.LearnIntent(intent);
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

    public static async Task ExecuteSingleTargetAttack(
        PlayerChoiceContext choiceContext,
        Creature attacker,
        Creature target,
        decimal damage,
        string attackerAnimName,
        float attackerAnimDelay,
        string hitVfxPath)
    {
        var attackerNode = NCombatRoom.Instance?.GetCreatureNode(attacker);
        if (attackerNode != null)
        {
            await attackerNode.PerformIntent();
        }

        await CreatureCmd.TriggerAnim(attacker, attackerAnimName, attackerAnimDelay);
        VfxCmd.PlayOnCreatureCenter(target, hitVfxPath);
        await CreatureCmd.Damage(choiceContext, target, damage, ValueProp.Move, attacker, null);
    }
}
