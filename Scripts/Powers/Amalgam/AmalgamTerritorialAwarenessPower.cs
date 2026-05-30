using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>领地意识：回合开始时，若聚合体存活，则使其获得力量。类名不可为 <c>TerritorialAwarenessPower</c>（与原版 ModelId 冲突）。</summary>
public sealed class AmalgamTerritorialAwarenessPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/territorial_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/territorial_power.png";

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _ = choiceContext;
        if (player != base.Owner.Player || !base.Owner.IsAlive || base.Owner.CombatState is not { } combatState)
        {
            return;
        }

        var amalgam = FriendlyAmalgamCmd.GetExisting(combatState, player);
        if (amalgam is not { IsAlive: true })
        {
            return;
        }

        if (Amount <= 0m)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, amalgam, Amount, base.Owner, null);
    }
}
