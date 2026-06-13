using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

public static class AmalgamActionRegistry
{
    private static AmalgamActionPoolModel Pool => ModelDb.Singleton<AmalgamActionPoolModel>();

    public static async Task ExecuteTemporaryAsync(
        PlayerChoiceContext choiceContext,
        Creature amalgam,
        AmalgamActionModel? intent)
    {
        if (intent == null)
        {
            return;
        }

        try
        {
            await intent.ExecuteAsync(choiceContext, amalgam);
        }
        finally
        {
            Return(intent);
        }
    }

    public static T? Rent<T>(decimal amount)
        where T : AmalgamActionModel, new()
    {
        T instance = Pool.Rent<T>();
        if (instance.Init(amount))
        {
            return instance;
        }

        Return(instance);
        return null;
    }

    public static T? Rent<T>(params object[] args)
        where T : AmalgamActionModel, new()
    {
        T instance = Pool.Rent<T>();
        if (instance.Init(args))
        {
            return instance;
        }

        Return(instance);
        return null;
    }

    public static void Return(AmalgamActionModel? instance)
    {
        if (instance == null)
        {
            return;
        }

        Pool.Return(instance);
    }

    public static void ClearPools() => Pool.Clear();

    public static AmalgamActionModel? Create(string actionId, decimal amount)
    {
        if (amount <= 0m)
        {
            return null;
        }

        return actionId switch
        {
            "offense" => Rent<AmalgamOffenseIntentAction>(amount),
            "block" => Rent<AmalgamGainBlockIntentAction>(amount),
            "vulnerable" => Rent<AmalgamApplyVulnerableIntentAction>(amount),
            "weak" => Rent<AmalgamApplyWeakIntentAction>(amount),
            "constrict" => new AmalgamApplyDebuffIntentAction<ConstrictPower>(
                amount,
                debuffEntryId: "CONSTRICT_POWER"),
            "shrink_ray" => Rent<AmalgamShrinkRayIntentAction>(amount),
            "strength_down" => Rent<AmalgamStrengthDownIntentAction>(amount),
            "strength" => Rent<AmalgamGainStrengthIntentAction>(amount),
            _ => null
        };
    }

    public static AmalgamActionModel? CreateOffense(decimal damage) =>
        Rent<AmalgamOffenseIntentAction>(damage);

    public static AmalgamActionModel? CreateOffense(decimal damage, Creature? forcedTarget) =>
        Rent<AmalgamOffenseIntentAction>(damage, forcedTarget!);

    public static AmalgamActionModel? CreateOffenseMulti(decimal damagePerHit, int hitCount) =>
        Rent<AmalgamMultiHitOffenseIntentAction>(damagePerHit, hitCount);

    public static AmalgamActionModel? CreateLashOffenseMulti(decimal damagePerHit, int hitCount) =>
        Rent<AmalgamLashMultiHitOffenseIntentAction>(damagePerHit, hitCount);

    public static AmalgamActionModel? CreateOffenseMultiAndWeak(decimal damagePerHit, int hitCount, decimal weakStacks) =>
        Rent<AmalgamMultiHitOffenseAndWeakIntentAction>(damagePerHit, hitCount, weakStacks);

    public static AmalgamActionModel? CreateBlock(decimal block) =>
        Rent<AmalgamGainBlockIntentAction>(block);

    public static AmalgamActionModel? CreateVulnerable(decimal stacks) =>
        Rent<AmalgamApplyVulnerableIntentAction>(stacks);

    public static AmalgamActionModel? CreateWeak(decimal stacks) =>
        Rent<AmalgamApplyWeakIntentAction>(stacks);

    public static AmalgamActionModel? CreateWeak(decimal stacks, Creature? forcedTarget) =>
        Rent<AmalgamApplyWeakIntentAction>(stacks, forcedTarget!);

    public static AmalgamActionModel? CreateConstrict(decimal stacks) => Create("constrict", stacks);

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

    public static AmalgamActionModel? CreateShrinkRay(decimal turns) =>
        Rent<AmalgamShrinkRayIntentAction>(turns);

    public static AmalgamActionModel? CreateStrengthDown(decimal strengthLoss) =>
        Rent<AmalgamStrengthDownIntentAction>(strengthLoss);

    public static AmalgamActionModel? CreateVulnerable(decimal stacks, Creature? forcedTarget) =>
        Rent<AmalgamApplyVulnerableIntentAction>(stacks, forcedTarget!);

    public static AmalgamActionModel? CreateStrength(decimal strength) =>
        Rent<AmalgamGainStrengthIntentAction>(strength);

    public static AmalgamActionModel? CreateGenerateCard<T>(decimal count, bool generateUpgradedCard = false)
        where T : QueenCardModel
    {
        return Rent<AmalgamGenerateCardIntentAction<T>>(count, generateUpgradedCard);
    }

    public static AmalgamActionModel? CreateDrawAndEnchant<TEnchantment>(decimal count)
        where TEnchantment : EnchantmentModel
    {
        return Rent<AmalgamDrawAndEnchantIntentAction<TEnchantment>>(count);
    }

    public static AmalgamActionModel? CreateDrawAndEnchantWithAmount<TEnchantment>(decimal count, decimal enchantAmount)
        where TEnchantment : EnchantmentModel
    {
        return Rent<AmalgamDrawAndEnchantWithAmountIntentAction<TEnchantment>>(count, enchantAmount);
    }

    public static AmalgamActionModel? CreateAttackAndStrength(decimal damage, decimal strength) =>
        Rent<AmalgamAttackAndStrengthIntentAction>(damage, strength);

    public static AmalgamActionModel? CreateAttackAndWeak(decimal damage, decimal weakStacks) =>
        Rent<AmalgamAttackAndWeakIntentAction>(damage, weakStacks);

    public static AmalgamActionModel? CreateAttackAndBlock(decimal damage, decimal block) =>
        Rent<AmalgamAttackAndBlockIntentAction>(damage, block);

    public static AmalgamActionModel? CreateGainBuff<TPower>(decimal stacks, string buffEntryId)
        where TPower : PowerModel
    {
        return Rent<AmalgamGainBuffIntentAction<TPower>>(stacks, buffEntryId);
    }
}
