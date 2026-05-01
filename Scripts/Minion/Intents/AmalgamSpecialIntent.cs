using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>特殊意图：unknown 动画 + unknown 图标；用于展示“特殊操作”。</summary>
public sealed class AmalgamSpecialIntent : AbstractIntent
{
    private readonly string _descriptionKey;

    public AmalgamSpecialIntent(string descriptionKey)
    {
        _descriptionKey = descriptionKey;
    }

    public override IntentType IntentType => IntentType.Buff;

    protected override string IntentPrefix => "AMALGAM_SPECIAL";

    protected override string SpritePath => "atlases/intent_atlas.sprites/intent_unknown.tres";

    public override string GetAnimation(IEnumerable<Creature> targets, Creature owner) => "unknown";

    public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner) =>
        new("intents", "FORMAT_EMPTY");

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner) =>
        new("intents", _descriptionKey);
}

