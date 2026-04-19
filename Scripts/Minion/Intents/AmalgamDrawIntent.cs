using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图条：下回合抽牌；卡牌减益图标。</summary>
public sealed class AmalgamDrawIntent : AbstractIntent
{
    public override IntentType IntentType => IntentType.CardDebuff;

    protected override string IntentPrefix => "AMALGAM_DRAW";

    protected override string SpritePath => "atlases/intent_atlas.sprites/intent_card_debuff.tres";

    public override string GetAnimation(IEnumerable<Creature> targets, Creature owner) => "card_debuff";

    public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner) =>
        new("intents", "FORMAT_AMALGAM_DRAW_LABEL");

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        LocString d = new("intents", "AMALGAM_DRAW.description");
        CombatState? combatState = owner.CombatState;
        d.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
        return d;
    }
}
