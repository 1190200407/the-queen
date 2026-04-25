using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>通用聚合体 Debuff 意图展示：对敌人施加若干层某种 Debuff。</summary>
public sealed class AmalgamApplyDebuffIntent : AbstractIntent
{
    private readonly int _stacks;
    private readonly string _debuffEntryId;

    public AmalgamApplyDebuffIntent(string debuffEntryId, decimal stacks)
    {
        _debuffEntryId = debuffEntryId;
        _stacks = System.Math.Max(0, (int)stacks);
    }

    public override IntentType IntentType => IntentType.Debuff;

    protected override string IntentPrefix => "AMALGAM_APPLY_DEBUFF";

    protected override string SpritePath => "atlases/intent_atlas.sprites/intent_debuff.tres";

    protected override LocString IntentLabelFormat => new("intents", "FORMAT_DEBUFF_STACKS");

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
        CombatState? combatState = owner.CombatState;
        d.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
        d.Add("Stacks", _stacks);
        d.Add("DebuffName", new LocString("powers", _debuffEntryId + ".title"));
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

