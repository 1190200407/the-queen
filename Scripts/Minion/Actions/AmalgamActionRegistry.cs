using System;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

public static class AmalgamActionRegistry
{
    public const string Distract = "distract";
    public const string AttackAndBlock = "attack_and_block";
    public const string AttackAndStrength = "attack_and_strength";
    public const string AttackAndWeak = "attack_and_weak";
    public const string Offense = "offense";
    public const string Block = "block";
    public const string Vulnerable = "vulnerable";
    public const string Weak = "weak";
    public const string Constrict = "constrict";
    public const string ShrinkRay = "shrink_ray";
    public const string StrengthDown = "strength_down";
    public const string Strength = "strength";

    public static AmalgamActionModel? Create(string actionId, decimal amount)
    {
        if (amount <= 0m)
        {
            return null;
        }

        return actionId switch
        {
            Offense => new AmalgamOffenseIntentAction(amount),
            Block => new AmalgamGainBlockIntentAction(amount),
            Vulnerable => new AmalgamApplyVulnerableIntentAction(amount),
            Weak => new AmalgamApplyWeakIntentAction(amount),
            Constrict => new AmalgamApplyDebuffIntentAction<ConstrictPower>(
                amount,
                debuffEntryId: "CONSTRICT_POWER"),
            ShrinkRay => new AmalgamShrinkRayIntentAction(amount),
            StrengthDown => new AmalgamStrengthDownIntentAction(amount),
            Strength => new AmalgamGainStrengthIntentAction(amount),
            _ => null
        };
    }

    public static AmalgamActionModel? CreateOffense(decimal damage) => Create(Offense, damage);

    public static AmalgamActionModel? CreateOffense(decimal damage, Creature? forcedTarget)
    {
        if (damage <= 0m)
        {
            return null;
        }

        return new AmalgamOffenseIntentAction(damage, forcedTarget);
    }

    public static AmalgamActionModel? CreateOffenseMulti(decimal damagePerHit, int hitCount)
    {
        if (damagePerHit <= 0m || hitCount <= 0)
        {
            return null;
        }

        return new AmalgamMultiHitOffenseIntentAction(damagePerHit, hitCount);
    }

    public static AmalgamActionModel? CreateLashOffenseMulti(decimal damagePerHit, int hitCount)
    {
        if (damagePerHit <= 0m || hitCount <= 0)
        {
            return null;
        }

        return new AmalgamLashMultiHitOffenseIntentAction(damagePerHit, hitCount);
    }

    public static AmalgamActionModel? CreateOffenseMultiAndWeak(decimal damagePerHit, int hitCount, decimal weakStacks)
    {
        if (damagePerHit <= 0m || hitCount <= 0 || weakStacks <= 0m)
        {
            return null;
        }

        return new AmalgamMultiHitOffenseAndWeakIntentAction(damagePerHit, hitCount, weakStacks);
    }

    public static AmalgamActionModel? CreateBlock(decimal block) => Create(Block, block);

    public static AmalgamActionModel? CreateVulnerable(decimal stacks) => Create(Vulnerable, stacks);

    public static AmalgamActionModel? CreateWeak(decimal stacks) => Create(Weak, stacks);
    
    public static AmalgamActionModel? CreateWeak(decimal stacks, Creature? forcedTarget)
    {
        if (stacks <= 0m)
        {
            return null;
        }

        return new AmalgamApplyWeakIntentAction(stacks, forcedTarget);
    }
    
    public static AmalgamActionModel? CreateConstrict(decimal stacks) => Create(Constrict, stacks);
    
    public static AmalgamActionModel? CreateConstrict(decimal stacks, Creature? forcedTarget)
    {
        if (stacks <= 0m)
        {
            return null;
        }

        return new AmalgamApplyDebuffIntentAction<ConstrictPower>(
            stacks,
            debuffEntryId: "CONSTRICT_POWER",
            forcedTarget);
    }
    
    public static AmalgamActionModel? CreateShrinkRay(decimal turns) => Create(ShrinkRay, turns);

    public static AmalgamActionModel? CreateStrengthDown(decimal strengthLoss) => Create(StrengthDown, strengthLoss);
    
    public static AmalgamActionModel? CreateVulnerable(decimal stacks, Creature? forcedTarget)
    {
        if (stacks <= 0m)
        {
            return null;
        }

        return new AmalgamApplyVulnerableIntentAction(stacks, forcedTarget);
    }

    public static AmalgamActionModel? CreateStrength(decimal strength) => Create(Strength, strength);
    public static AmalgamActionModel? CreateGenerateCard<T>(decimal count, bool generateUpgradedCard = false) where T : QueenCardModel
    {
        if (count <= 0m)
        {
            return null;
        }

        return new AmalgamGenerateCardIntentAction<T>(count, generateUpgradedCard);
    }

    public static AmalgamActionModel? CreateDrawAndEnchant<TEnchantment>(decimal count)
        where TEnchantment : EnchantmentModel
    {
        if (count <= 0m)
        {
            return null;
        }

        return new AmalgamDrawAndEnchantIntentAction<TEnchantment>(count);
    }

    public static AmalgamActionModel? CreateDrawAndEnchantWithAmount<TEnchantment>(decimal count, decimal enchantAmount)
        where TEnchantment : EnchantmentModel
    {
        if (count <= 0m || enchantAmount <= 0m)
        {
            return null;
        }

        return new AmalgamDrawAndEnchantWithAmountIntentAction<TEnchantment>(count, enchantAmount);
    }

    public static AmalgamActionModel? CreateAttackAndStrength(decimal damage, decimal strength)
    {
        if (damage <= 0m || strength <= 0m)
        {
            return null;
        }

        return new AmalgamAttackAndStrengthIntentAction(damage, strength);
    }

    public static AmalgamActionModel? CreateAttackAndWeak(decimal damage, decimal weakStacks)
    {
        if (damage <= 0m || weakStacks <= 0m)
        {
            return null;
        }

        return new AmalgamAttackAndWeakIntentAction(damage, weakStacks);
    }

    public static AmalgamActionModel? CreateAttackAndBlock(decimal damage, decimal block)
    {
        if (damage <= 0m || block <= 0m)
        {
            return null;
        }

        return new AmalgamAttackAndBlockIntentAction(damage, block);
    }

    // Stun 意图已改为特殊意图（unknown）实现；不再提供 CreateStun。

    public static AmalgamActionModel CreateEmptyCup() => new AmalgamEmptyCupIntentAction();

    public static AmalgamActionModel? CreateGainBuff<TPower>(decimal stacks, string buffEntryId)
        where TPower : PowerModel
    {
        if (stacks <= 0m)
        {
            return null;
        }

        return new AmalgamGainBuffIntentAction<TPower>(stacks, buffEntryId);
    }
}

