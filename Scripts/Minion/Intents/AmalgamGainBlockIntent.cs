using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>友方聚合体格挡意图；意图条数字用 <c>intents</c> 表 <c>FORMAT_BLOCK_SINGLE</c>。</summary>
public sealed class AmalgamGainBlockIntent : AbstractIntent
{
	private readonly int _block;

	public AmalgamGainBlockIntent(decimal block)
	{
		_block = System.Math.Max(0, (int)block);
	}

	public override IntentType IntentType => IntentType.Defend;

	protected override string IntentPrefix => "AMALGAM_GAIN_BLOCK";

	protected override string SpritePath => "atlases/intent_atlas.sprites/intent_defend.tres";

	public override string GetAnimation(IEnumerable<Creature> targets, Creature owner) => "defend";

	public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
	{
		var fmt = new LocString("intents", "FORMAT_BLOCK_SINGLE");
		fmt.Add("Block", _block);
		return fmt;
	}

	protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
	{
		LocString d = new("intents", IntentPrefix + ".description");
		ICombatState? combatState = owner.CombatState;
		d.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
		d.Add("Block", _block);
		return d;
	}
}
