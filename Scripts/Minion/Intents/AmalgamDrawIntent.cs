using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图条：抽牌。</summary>
public sealed class AmalgamDrawIntent : AmalgamDrawAndEnchantIntent
{
    public AmalgamDrawIntent(decimal count = 1m)
        : base(count, enchantmentId: null)
    {
    }
}

/// <summary>聚合体意图条：抽牌并附魔。</summary>
public class AmalgamDrawAndEnchantIntent : AbstractIntent
{
    private readonly int _count;
    private readonly string? _enchantmentId;
    public AmalgamDrawAndEnchantIntent(decimal count = 1m, string? enchantmentId = null)
    {
        _count = System.Math.Max(0, (int)count);
        _enchantmentId = enchantmentId;
    }

    public override IntentType IntentType => IntentType.StatusCard;

    protected override string IntentPrefix => "AMALGAM_DRAW";

    protected override string SpritePath => "atlases/intent_atlas.sprites/intent_status_card.tres";

    public override string GetAnimation(IEnumerable<Creature> targets, Creature owner) => "status";

    public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
    {
        LocString fmt = new("intents", "FORMAT_AMALGAM_DRAW_LABEL");
        fmt.Add("Count", _count);
        return fmt;
    }

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        LocString d = new("intents", "AMALGAM_DRAW.description");
        CombatState? combatState = owner.CombatState;
        d.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
        d.Add("Count", _count);
        d.Add("Enchantment", new LocString("enchantments", _enchantmentId + ".title"));
        return d;
    }
}
