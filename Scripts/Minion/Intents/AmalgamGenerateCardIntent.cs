using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>友方聚合体：在手牌中加入指定卡牌的意图展示。</summary>
public sealed class AmalgamGenerateCardIntent : AbstractIntent
{
    private readonly int _count;
    private readonly string _cardName;

    public AmalgamGenerateCardIntent(decimal count, string cardName)
    {
        _count = System.Math.Max(0, (int)count);
        _cardName = cardName;
    }

    public override IntentType IntentType => IntentType.Summon;

    protected override string IntentPrefix => "AMALGAM_GENERATE_CARD";

    // 使用原版怪物「召唤爪牙」意图图标。
    protected override string SpritePath => "atlases/intent_atlas.sprites/intent_summon.tres";

    protected override LocString IntentLabelFormat => new("intents", "FORMAT_AMALGAM_GENERATE_CARD_LABEL");

    public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
    {
        LocString fmt = IntentLabelFormat ?? new LocString("intents", "FORMAT_EMPTY");
        fmt.Add("Count", _count);
        return fmt;
    }

    public override string GetAnimation(IEnumerable<Creature> targets, Creature owner) => "summon";

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        LocString d = new("intents", IntentPrefix + ".description");
        ICombatState? combatState = owner.CombatState;
        d.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
        d.Add("Count", _count);
        d.Add("CardName", _cardName);
        return d;
    }
}
