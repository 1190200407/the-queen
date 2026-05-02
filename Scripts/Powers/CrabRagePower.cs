using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>蟹之怒（女王版）：聚合体死亡时，你获得力量与格挡，然后移除此能力。</summary>
public sealed class CrabRagePower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    // 复用原版 Crab Rage 的图标资源。
    public override string? CustomPackedIconPath => "res://images/atlases/power_atlas.sprites/crab_rage_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/crab_rage_power.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(3m),
        new BlockVar(30m, ValueProp.Unpowered),
    ];

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        _ = choiceContext;
        _ = deathAnimLength;
        if (wasRemovalPrevented || !base.Owner.IsAlive)
        {
            return;
        }

        // 仅在聚合体死亡时触发（聚合体是女王的友方随从）。
        if (creature is not { IsAlive: false } || creature.PetOwner != base.Owner.Player)
        {
            return;
        }
        if (creature.Monster is not FriendlyAmalgam)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<StrengthPower>(base.Owner, base.DynamicVars.Strength.IntValue, base.Owner, null);
        await CreatureCmd.GainBlock(base.Owner, base.DynamicVars.Block, null);
        await PowerCmd.Remove(this);
    }
}

