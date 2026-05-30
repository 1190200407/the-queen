using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>蟹之怒（女王版）：聚合体沉睡时，你获得力量与格挡，然后移除此能力。</summary>
public sealed class AmalgamCrabRagePower : QueenPowerModel, IAmalgamEventListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/crab_rage_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/crab_rage_power.png";

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(3m),
        new BlockVar(30m, ValueProp.Unpowered),
    ];

    public async Task OnAmalgamFallAsleepAsync(CombatState combatState, Creature amalgam)
    {
        _ = combatState;
        if (!base.Owner.IsAlive || amalgam.PetOwner != base.Owner.Player || amalgam.Monster is not FriendlyAmalgam)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<StrengthPower>(
            new ThrowingPlayerChoiceContext(),
            base.Owner,
            base.DynamicVars.Strength.IntValue,
            base.Owner,
            null);
        await CreatureCmd.GainBlock(base.Owner, base.DynamicVars.Block, null);
        await PowerCmd.Remove(this);
    }
}
