using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>友方聚合体多段进攻意图；单次伤害预览与 <see cref="AmalgamSingleAttackIntent"/> 一致，总伤为单次×次数。</summary>
public sealed class AmalgamMultiHitAttackIntent : AttackIntent
{
	protected override string IntentPrefix => "AMALGAM_MULTI_ATTACK";

	private readonly decimal _baseDamage;
	private readonly int _repeats;

	public AmalgamMultiHitAttackIntent(decimal baseDamage, int repeats)
	{
		_baseDamage = baseDamage;
		_repeats = Math.Max(1, repeats);
		DamageCalc = () => _baseDamage;
	}

	public override int Repeats => _repeats;

	protected override LocString IntentLabelFormat => new("intents", "FORMAT_DAMAGE_MULTI");

	public override string GetAnimation(IEnumerable<Creature> targets, Creature owner)
	{
		const string animation = "attack";
		int totalDamage = GetTotalDamage(targets, owner);
		if (totalDamage < 5)
		{
			return animation + "_1";
		}

		if (totalDamage < 10)
		{
			return animation + "_2";
		}

		if (totalDamage < 20)
		{
			return animation + "_3";
		}

		if (totalDamage < 40)
		{
			return animation + "_4";
		}

		return animation + "_5";
	}

	public override int GetTotalDamage(IEnumerable<Creature> targets, Creature owner) =>
		Math.Max(0, GetOneHitConsensusDamage(targets, owner) * _repeats);

	public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
	{
		LocString intentLabelFormat = IntentLabelFormat ?? new LocString("intents", "FORMAT_EMPTY");
		intentLabelFormat.Add("Damage", GetOneHitConsensusDamage(targets, owner));
		intentLabelFormat.Add("Repeat", Repeats);
		return intentLabelFormat;
	}

	protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
	{
		string descKey = IntentPrefix + ResolveDescriptionSuffix(owner);
		LocString intentDescription = new("intents", descKey);
		ICombatState? combatState = owner.CombatState;
		intentDescription.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
		intentDescription.Add("Damage", GetOneHitConsensusDamage(targets, owner));
		intentDescription.Add("Repeat", Repeats);
		return intentDescription;
	}

	private int GetOneHitConsensusDamage(IEnumerable<Creature> targets, Creature owner)
	{
		if (owner.CombatState is not { } cs || !owner.IsAlive)
		{
			return Math.Max(0, (int)_baseDamage);
		}

		Creature[] pool = owner.PetOwner is Player queen
			? AmalgamOffenseTargeting.GetPreviewReceiverPool(cs, queen)
			: cs.Enemies.Where(e => e.IsAlive).ToArray();
		decimal display = pool.Length == 0
			? _baseDamage
			: AmalgamIntentDamagePreview.PreviewOutgoingConsensusAmongReceivers(
				owner,
				pool,
				_baseDamage,
				ValueProp.Move,
				cardSource: null,
				CardPreviewMode.None);
		return Math.Max(0, (int)display);
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
