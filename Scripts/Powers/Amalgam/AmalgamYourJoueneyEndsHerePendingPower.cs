using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>你的旅程，到此为止：聚合体沉睡倒计时结束后，获得配置的力量。类名不可为 <c>YourJoueneyEndsHerePendingPower</c>（与原版 ModelId 冲突）。</summary>
public sealed class AmalgamYourJoueneyEndsHerePendingPower : QueenPowerModel, IAmalgamEventListener
{
    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/conqueror_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/conqueror_power.png";

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

    public async Task AfterAmalgamTurnEnd(CombatState combatState, Creature amalgam)
    {
        await PowerCmd.Decrement(this);
        if (Amount > 0m)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        if (combatState != null)
        {
            Creature? amalgamCreature = base.Owner;
            if (amalgamCreature is { IsAlive: true } && data.StrengthToGain > 0m)
            {
                Flash();
                await PowerCmd.Apply<StrengthPower>(amalgamCreature, data.StrengthToGain, base.Owner, null);
                
                LocString line = MonsterModel.L10NMonsterLookup("FRIENDLY_AMALGAM.YOUR_JOURNEY_ENDS_HERE.speakLine2");
                ThinkCmd.Play(line, amalgamCreature);
            }
        }

        await PowerCmd.Remove(this);
    }
}
