using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>你的旅程，到此为止：聚合体沉睡倒计时结束后，获得配置的力量。</summary>
public sealed class YourJoueneyEndsHerePendingPower : QueenPowerModel
{
    // 复用原版沉睡图标。
    public override string? CustomPackedIconPath => "res://images/atlases/power_atlas.sprites/asleep_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/asleep_power.png";

    private sealed class Data
    {
        public decimal StrengthToGain = 0m;
    }

    protected override object? InitInternalData() => new Data();

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("StrengthToGain", 0m),
    ];

    public override bool IsInstanced => true;

    internal void ConfigureStrength(decimal strengthToGain)
    {
        Data data = GetInternalData<Data>();
        data.StrengthToGain = strengthToGain;
        base.DynamicVars["StrengthToGain"].BaseValue = strengthToGain;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await PowerCmd.Decrement(this);
        if (Amount > 0m)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        CombatState? combatState = base.Owner.CombatState;
        if (combatState != null)
        {
            Creature? amalgamCreature = FriendlyAmalgamCmd.GetExisting(combatState, player);
            if (amalgamCreature?.Monster is FriendlyAmalgam amalgam)
            {
                await amalgam.WakeUp(FriendlyAmalgam.SleepReason.YourTourEndsHere);
            }

            if (amalgamCreature is { IsAlive: true } && data.StrengthToGain > 0m)
            {
                Flash();
                await PowerCmd.Apply<StrengthPower>(amalgamCreature, data.StrengthToGain, base.Owner, null);
            }
        }

        await PowerCmd.Remove(this);
    }
}
