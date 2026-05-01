using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>领地意识：回合开始时，若聚合体存活，则使其获得 <see cref="PowerModel.Amount"/> 点力量。</summary>
public sealed class TerritorialAwarenessPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 尝试复用原版 Territorial 的图标资源；若运行时不可用，可改回本 mod 自带图。
    public override string? CustomPackedIconPath => "res://images/atlases/power_atlas.sprites/territorial_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/territorial_power.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

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
        await PowerCmd.Apply<StrengthPower>(amalgam, Amount, base.Owner, null);
    }
}
