using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>友方聚合体：对所有敌方单位施加若干层缩小的意图展示。</summary>
public sealed class AmalgamShrinkRayIntent : AbstractIntent
{
    private readonly int _turns;

    public AmalgamShrinkRayIntent(decimal turns)
    {
        _turns = System.Math.Max(0, (int)turns);
    }

    public override IntentType IntentType => IntentType.Debuff;

    protected override string IntentPrefix => "AMALGAM_SHRINK_RAY";

    protected override string SpritePath => "atlases/intent_atlas.sprites/intent_debuff.tres";

    protected override LocString IntentLabelFormat => new("intents", "FORMAT_DEBUFF_STACKS");

    public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
    {
        LocString fmt = IntentLabelFormat ?? new LocString("intents", "FORMAT_EMPTY");
        fmt.Add("Stacks", _turns);
        return fmt;
    }

    public override string GetAnimation(IEnumerable<Creature> targets, Creature owner) => "debuff";

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        LocString d = new("intents", "AMALGAM_SHRINK_RAY.description");
        ICombatState? combatState = owner.CombatState;
        d.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
        d.Add("Stacks", _turns);
        return d;
    }
}

