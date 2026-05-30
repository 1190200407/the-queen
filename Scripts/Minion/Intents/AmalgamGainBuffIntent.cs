using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>通用聚合体 Buff 意图展示：聚合体获得若干层某种 Buff。</summary>
public sealed class AmalgamGainBuffIntent : AbstractIntent
{
    private readonly int _stacks;
    private readonly string _buffEntryId;

    public AmalgamGainBuffIntent(string buffEntryId, decimal stacks)
    {
        _buffEntryId = buffEntryId;
        _stacks = System.Math.Max(0, (int)stacks);
    }

    public override IntentType IntentType => IntentType.Buff;

    protected override string IntentPrefix => "AMALGAM_GAIN_BUFF";

    protected override string SpritePath => "atlases/intent_atlas.sprites/intent_buff.tres";

    protected override LocString IntentLabelFormat => new("intents", "FORMAT_DEBUFF_STACKS");

    public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
    {
        LocString fmt = IntentLabelFormat ?? new LocString("intents", "FORMAT_EMPTY");
        fmt.Add("Stacks", _stacks);
        return fmt;
    }

    public override string GetAnimation(IEnumerable<Creature> targets, Creature owner) => "buff";

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        LocString d = new("intents", IntentPrefix + ".description");
        CombatState? combatState = owner.CombatState;
        d.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
        d.Add("Stacks", _stacks);
        d.Add("BuffName", new LocString("powers", _buffEntryId + ".title"));
        return d;
    }
}

