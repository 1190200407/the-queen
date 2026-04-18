using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ComicChess.TheQueen;

/// <summary>友方聚合体沉睡意图；文案用 mod <c>intents</c> 表 <c>AMALGAM_SLEEP.*</c>，动画名仍须为 <c>sleep</c>（勿用 IntentPrefix 当动画基底）。</summary>
public sealed class AmalgamSleepIntent : AbstractIntent
{
	protected override string IntentPrefix => "AMALGAM_SLEEP";

	protected override string? SpritePath => "atlases/intent_atlas.sprites/intent_sleep.tres";

	public override IntentType IntentType => IntentType.Sleep;

	public override string GetAnimation(IEnumerable<Creature> targets, Creature owner) => "sleep";
}
