using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图条：抽牌并附魔（带附魔层数）。</summary>
public sealed class AmalgamDrawAndEnchantWithAmountIntent<TEnchantment> : AbstractIntent
    where TEnchantment : EnchantmentModel
{
    private readonly int _count;
    private readonly string _enchantmentId;
    private readonly int _enchantAmount;

    public AmalgamDrawAndEnchantWithAmountIntent(decimal count, decimal enchantAmount)
    {
        _count = System.Math.Max(0, (int)count);
        _enchantmentId = ModelDb.Enchantment<TEnchantment>().Id.Entry;
        _enchantAmount = System.Math.Max(0, (int)enchantAmount);
    }

    public override IntentType IntentType => IntentType.StatusCard;

    protected override string IntentPrefix => "AMALGAM_DRAW_ENCHANT_AMOUNT";

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
        LocString d = new("intents", IntentPrefix + ".description");
        CombatState? combatState = owner.CombatState;
        d.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
        d.Add("Count", _count);
        d.Add("Enchantment", new LocString("enchantments", _enchantmentId + ".title"));
        d.Add("EnchantAmount", _enchantAmount);
        return d;
    }
}

