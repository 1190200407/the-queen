using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>友方聚合体：对敌人施加若干层 <see cref="MegaCrit.Sts2.Core.Models.Powers.WeakPower"/> 的意图展示。</summary>
public sealed class AmalgamApplyWeakIntent : AbstractIntent
{
    private readonly int _stacks;

    public AmalgamApplyWeakIntent(decimal stacks)
    {
        _stacks = System.Math.Max(0, (int)stacks);
    }

    public override IntentType IntentType => IntentType.Debuff;

    protected override string IntentPrefix => "AMALGAM_APPLY_WEAK";

    protected override string SpritePath => "atlases/intent_atlas.sprites/intent_debuff.tres";

    protected override LocString IntentLabelFormat => new("intents", "FORMAT_WEAK_STACKS");

    public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
    {
        LocString fmt = IntentLabelFormat ?? new LocString("intents", "FORMAT_EMPTY");
        fmt.Add("Stacks", _stacks);
        return fmt;
    }

    public override string GetAnimation(IEnumerable<Creature> targets, Creature owner) => "debuff";

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        string descKey = IntentPrefix + ResolveDescriptionSuffix(owner);
        LocString d = new("intents", descKey);
        ICombatState? combatState = owner.CombatState;
        d.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
        d.Add("Stacks", _stacks);
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

