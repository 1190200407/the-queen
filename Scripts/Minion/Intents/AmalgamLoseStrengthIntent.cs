using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>友方聚合体：目标永久失去力量（展示）。</summary>
public sealed class AmalgamLoseStrengthIntent : AbstractIntent
{
    private readonly int _stacks;

    public AmalgamLoseStrengthIntent(decimal stacks)
    {
        _stacks = System.Math.Max(0, (int)stacks);
    }

    public override IntentType IntentType => IntentType.Debuff;

    protected override string IntentPrefix => "AMALGAM_LOSE_STRENGTH";

    protected override string SpritePath => "atlases/intent_atlas.sprites/intent_debuff.tres";

    protected override LocString IntentLabelFormat => new("intents", "FORMAT_STRENGTH_STACKS");

    public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
    {
        LocString fmt = IntentLabelFormat ?? new LocString("intents", "FORMAT_EMPTY");
        fmt.Add("Stacks", _stacks);
        return fmt;
    }

    public override string GetAnimation(IEnumerable<Creature> targets, Creature owner) => "debuff";

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        LocString d = new("intents", IntentPrefix + ".description");
        CombatState? combatState = owner.CombatState;
        d.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
        d.Add("Stacks", _stacks);
        return d;
    }
}

