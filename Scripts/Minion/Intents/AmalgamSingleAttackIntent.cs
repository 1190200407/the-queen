using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>
/// 友方聚合体进攻意图；展示用伤害在 <see cref="GetTotalDamage"/> 内按 <see cref="AmalgamOffenseTargeting"/> 与敌共识算一次。
/// 不可用 <see cref="SingleAttackIntent"/>：<see cref="AttackIntent.GetSingleDamage"/> 把玩家当承伤者，误叠我方易伤，且与预计算数值双算。
/// </summary>
public sealed class AmalgamSingleAttackIntent : AttackIntent
{
	/// <summary>对应 mod <c>TheQueen/localization/*/intents.json</c> 中的 <c>AMALGAM_SINGLE_ATTACK.*</c>。</summary>
	protected override string IntentPrefix => "AMALGAM_SINGLE_ATTACK";

	private readonly decimal _baseDamage;

	public AmalgamSingleAttackIntent(decimal baseDamage)
	{
		_baseDamage = baseDamage;
		DamageCalc = () => _baseDamage;
	}

	public override int Repeats => 1;

	protected override LocString IntentLabelFormat => new("intents", "FORMAT_DAMAGE_SINGLE");

	/// <summary>
	/// 必须用 <c>attack</c> 前缀：原版 <see cref="AttackIntent.GetAnimation"/> 依赖 <see cref="AbstractIntent.IntentPrefix"/>，
	/// 改成 <c>AMALGAM_SINGLE_ATTACK</c> 后 <c>NIntent</c> 会去 <see cref="IntentAnimData"/> 找不存在的序列，图标动画会坏。
	/// </summary>
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

	public override int GetTotalDamage(IEnumerable<Creature> targets, Creature owner)
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

	/// <summary>与 <see cref="GetTotalDamage"/> 一致，避免外部误调基类实现。</summary>
	public new int GetSingleDamage(IEnumerable<Creature> targets, Creature owner) =>
		GetTotalDamage(targets, owner);

	public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
	{
		LocString intentLabelFormat = IntentLabelFormat ?? new LocString("intents", "FORMAT_EMPTY");
		intentLabelFormat.Add("Damage", GetTotalDamage(targets, owner));
		return intentLabelFormat;
	}

	protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
	{
		string descKey = IntentPrefix + ResolveDescriptionSuffix(owner);
		LocString intentDescription = new("intents", descKey);
		CombatState? combatState = owner.CombatState;
		intentDescription.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
		intentDescription.Add("Damage", GetTotalDamage(targets, owner));
		intentDescription.Add("Repeat", Repeats);
		return intentDescription;
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
