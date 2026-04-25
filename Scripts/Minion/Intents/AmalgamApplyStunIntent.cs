using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>友方聚合体：对敌人施加 1 回合晕眩的意图展示。</summary>
public sealed class AmalgamApplyStunIntent : AbstractIntent
{
    public override IntentType IntentType => IntentType.Debuff;

    protected override string IntentPrefix => "AMALGAM_APPLY_STUN";

    protected override string SpritePath => "atlases/intent_atlas.sprites/intent_debuff.tres";

    protected override LocString IntentLabelFormat => new("intents", "FORMAT_EMPTY");

    public override string GetAnimation(IEnumerable<Creature> targets, Creature owner) => "debuff";

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        string descKey = IntentPrefix + ResolveDescriptionSuffix(owner);
        LocString d = new("intents", descKey);
        CombatState? combatState = owner.CombatState;
        d.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
        return d;
    }

    private static string ResolveDescriptionSuffix(Creature amalgamOwner)
    {
        if (amalgamOwner.CombatState is not { } cs || amalgamOwner.PetOwner is not Player queen)
        {
            return ".description";
        }

        AmalgamOffenseTargetingMode mode = AmalgamOffenseTargeting.ResolveMode(cs, queen);
        return mode == AmalgamOffenseTargetingMode.AllAliveEnemies ? ".description_all" : ".description";
    }
}
