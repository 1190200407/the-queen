using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图条：抽牌（含下回合抽牌）并为抽到的牌附魔。</summary>
public sealed class AmalgamNoiseIntent : AbstractIntent
{
    private readonly string _enchantmentId;

    public AmalgamNoiseIntent(string enchantmentId)
    {
        _enchantmentId = enchantmentId;
    }

    public override IntentType IntentType => IntentType.StatusCard;

    protected override string IntentPrefix => "AMALGAM_NOISE";

    protected override string SpritePath => "atlases/intent_atlas.sprites/intent_status_card.tres";

    public override string GetAnimation(IEnumerable<Creature> targets, Creature owner) => "status";

    public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
    {
        LocString fmt = new("intents", "FORMAT_AMALGAM_DRAW_LABEL");
        fmt.Add("Count", 2);
        return fmt;
    }

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        LocString d = new("intents", IntentPrefix + ".description");
        CombatState? combatState = owner.CombatState;
        d.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
        d.Add("Enchantment", new LocString("enchantments", _enchantmentId + ".title"));
        return d;
    }
}

