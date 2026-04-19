using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;

namespace ComicChess.TheQueen;

/// <summary>意图/怪物文案里的 <c>{energyPrefix:energyIcons(n)}</c> 需要显式注入 <c>energyPrefix</c>（与 <see cref="MegaCrit.Sts2.Core.Models.PowerModel"/> 的说明变量一致）。</summary>
internal static class AmalgamIntentEnergyLoc
{
    internal static void AddEnergyPrefixFromPetOwner(LocString loc, Creature amalgam)
    {
        if (amalgam.PetOwner?.Character.CardPool is { } pool)
        {
            loc.Add("energyPrefix", EnergyIconHelper.GetPrefix(pool));
        }
        else
        {
            loc.Add("energyPrefix", "colorless");
        }
    }
}
