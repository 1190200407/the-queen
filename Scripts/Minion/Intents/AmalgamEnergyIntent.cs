using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图条：下回合能量；增益图标 + 能量图标数字。</summary>
public sealed class AmalgamEnergyIntent : AbstractIntent
{
    public override IntentType IntentType => IntentType.Buff;

    protected override string IntentPrefix => "AMALGAM_ENERGY";

    protected override string SpritePath => "atlases/intent_atlas.sprites/intent_buff.tres";

    public override string GetAnimation(IEnumerable<Creature> targets, Creature owner) => "buff";

    public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
    {
        LocString fmt = new("intents", "FORMAT_AMALGAM_ENERGY_LABEL");
        AmalgamIntentEnergyLoc.AddEnergyPrefixFromPetOwner(fmt, owner);
        return fmt;
    }

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        LocString d = new("intents", "AMALGAM_ENERGY.description");
        CombatState? combatState = owner.CombatState;
        d.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
        AmalgamIntentEnergyLoc.AddEnergyPrefixFromPetOwner(d, owner);
        return d;
    }
}
