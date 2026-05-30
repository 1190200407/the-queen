using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>友方聚合体：所有敌人永久失去力量（展示）。</summary>
public sealed class AmalgamLoseStrengthAllIntent : AbstractIntent
{
    private readonly int _stacks;

    public AmalgamLoseStrengthAllIntent(decimal stacks)
    {
        _stacks = System.Math.Max(0, (int)stacks);
    }

    public override IntentType IntentType => IntentType.Debuff;

    protected override string IntentPrefix => "AMALGAM_LOSE_STRENGTH_ALL";

    protected override string SpritePath => "atlases/intent_atlas.sprites/intent_debuff.tres";

    protected override LocString IntentLabelFormat => new("intents", "FORMAT_STRENGTH_STACKS");

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
        return d;
    }

    private static string ResolveDescriptionSuffix(Creature amalgamOwner)
    {
        if (amalgamOwner.CombatState is not { } cs || amalgamOwner.PetOwner is not Player queen)
        {
            return ".description";
        }

        // 固定全体敌人，不受索敌影响；但仍保留 _all 变体给文案一致性。
        _ = AmalgamOffenseTargeting.ResolveMode(cs, queen);
        return ".description_all";
    }
}

