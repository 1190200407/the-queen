using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace ComicChess.TheQueen;

/// <summary>
/// PersonalHivePower：与原版奥斯提一致，聚合体作为出手者时将“伤害归因/处理对象”转移给主人。
/// 只影响 PersonalHivePower.AfterDamageReceived 内部看到的参数，不改实际伤害结算与表现。
/// </summary>
[HarmonyPatch]
internal static class PersonalHivePowerAmalgamDealerTransferPatch
{
    private static MethodBase? TargetMethod()
    {
        Type? t = AccessTools.TypeByName("MegaCrit.Sts2.Core.Models.Powers.PersonalHivePower");
        return t == null ? null : AccessTools.Method(t, "AfterDamageReceived");
    }

    [HarmonyPrefix]
    private static void AfterDamageReceived_Prefix(object[] __args)
    {
        // 兼容不同版本签名：扫描所有参数，找到“聚合体作为 Creature 传入”的那个，
        // 替换为其主人 Creature，让 PersonalHivePower 的归因逻辑按主人处理。
        for (int i = 0; i < __args.Length; i++)
        {
            if (__args[i] is not Creature dealer)
            {
                continue;
            }

            if (dealer.Monster is not FriendlyAmalgam)
            {
                continue;
            }

            Creature? ownerCreature = dealer.PetOwner?.Creature;
            if (ownerCreature == null)
            {
                continue;
            }

            __args[i] = ownerCreature;
            return;
        }
    }
}

